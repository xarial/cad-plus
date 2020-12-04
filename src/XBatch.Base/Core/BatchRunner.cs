//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchRunner : IDisposable
    {
        private const int MAX_ATTEMPTS = 3;

        private readonly TextWriter m_UserLogger;
        private readonly IProgressHandler m_ProgressHandler;
        private readonly IApplicationProvider m_AppProvider;
        private readonly IMacroRunnerExService m_MacroRunnerSvc;

        private readonly IXLogger m_Logger;
        private readonly IJobManager m_JobMgr;

        public BatchRunner(IApplicationProvider appProvider, 
            IMacroRunnerExService macroRunnerSvc,
            TextWriter userLogger, IProgressHandler progressHandler, IJobManager jobMgr, IXLogger logger)
        {
            m_UserLogger = userLogger;
            m_ProgressHandler = progressHandler;
            m_MacroRunnerSvc = macroRunnerSvc;
            m_AppProvider = appProvider;

            m_Logger = logger;

            m_JobMgr = jobMgr;
        }

        public async Task<bool> BatchRun(BatchJob opts, CancellationToken cancellationToken = default)
        {
            m_UserLogger.WriteLine($"Batch macro running started");

            var batchStartTime = DateTime.Now;
            
            var allFiles = PrepareJobScope(opts.Input, opts.Filters, opts.Macros).ToArray();

            m_UserLogger.WriteLine($"Running batch processing for {allFiles.Length} file(s)");

            m_ProgressHandler.SetJobScope(allFiles, batchStartTime);

            if (!allFiles.Any())
            {
                throw new UserMessageException("Empty job. No files matching specified filter");
            }

            TimeSpan timeout = default;

            if (opts.Timeout > 0)
            {
                timeout = TimeSpan.FromSeconds(opts.Timeout);
            }

            IXApplication app = null;
            Process appPrc = null;

            if (cancellationToken != default)
            {
                cancellationToken.Register(() =>
                {
                    m_UserLogger.WriteLine($"Cancelled by the user");
                    TryShutDownApplication(appPrc);
                });
            }

            var jobResult = false;

            try
            {
                await Task.Run(() =>
                {
                    var curBatchSize = 0;

                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        var curAppPrc = appPrc?.Id;

                        var curFile = allFiles[i];
                        var res = AttemptProcessFile(ref app, ref appPrc, curFile, opts, cancellationToken);
                        m_ProgressHandler?.ReportProgress(curFile, res);

                        if (!res && !opts.ContinueOnError) 
                        {
                            throw new UserMessageException("Cancelling the job. Set 'Continue On Error' option to continue job if file failed");
                        }

                        if (appPrc?.Id != curAppPrc)
                        {
                            curBatchSize = 1;
                        }
                        else
                        {
                            curBatchSize++;
                        }

                        if (opts.BatchSize > 0 && curBatchSize >= opts.BatchSize && !cancellationToken.IsCancellationRequested) 
                        {
                            m_UserLogger.WriteLine("Closing application as batch size reached the limit");
                            TryShutDownApplication(appPrc);
                            curBatchSize = 0;
                        }
                    }
                }).ConfigureAwait(false);
                jobResult = true;
            }
            finally
            {
                TryShutDownApplication(appPrc);
            }

            var duration = DateTime.Now.Subtract(batchStartTime);

            m_ProgressHandler.ReportCompleted(duration);

            m_UserLogger.WriteLine($"Batch running completed in {duration.ToString(@"hh\:mm\:ss")}");

            return jobResult;
        }

        private bool MatchesFilter(string file, string[] filters) 
        {
            if (filters?.Any() == false)
            {
                return true;
            }
            else 
            {
                const string ANY_FILTER = "*";

                return filters.Any(f => 
                {
                    var regex = (f.StartsWith(ANY_FILTER) ? "" : "^")
                    + Regex.Escape(f).Replace($"\\{ANY_FILTER}", ".*").Replace("\\?", ".")
                    + (f.EndsWith(ANY_FILTER) ? "" : "$");

                    return Regex.IsMatch(file, regex, RegexOptions.IgnoreCase);
                });
            }
        }

        private IEnumerable<JobItemFile> PrepareJobScope(IEnumerable<string> inputs,  
            string[] filters, IEnumerable<MacroData> macros) 
        {
            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    foreach (var file in Directory.EnumerateFiles(input, "*.*", SearchOption.AllDirectories))
                    {
                        if (MatchesFilter(file, filters))
                        {
                            if (m_AppProvider.CanProcessFile(file))
                            {
                                yield return new JobItemFile(file, macros.Select(m => new JobItemMacro(m)).ToArray());
                            }
                            else 
                            {
                                m_UserLogger.WriteLine($"Skipping file '{file}'");
                            }
                        }
                    }
                }
                else if (File.Exists(input))
                {
                    yield return new JobItemFile(input, macros.Select(m => new JobItemMacro(m)).ToArray());
                }
                else
                {
                    throw new Exception("Specify input file or directory");
                }
            }
        }

        private void TryCloseDocument(IXDocument doc)
        {
            if (doc != null)
            {
                try
                {
                    doc.Close();
                }
                catch
                {
                }
            }
        }

        private void TryShutDownApplication(Process appPrc)
        {
            try
            {
                if (appPrc != null)
                {
                    if (!appPrc.HasExited)
                    {
                        m_UserLogger.WriteLine("Closing host application");
                        appPrc.Kill();
                    }
                }
            }
            catch 
            {
            }
        }

        private IXApplication AttemptStartApplication(AppVersionInfo versionInfo, StartupOptions_e opts, 
            CancellationToken cancellationToken, TimeSpan? timeout) 
        {   
            int curAttempt = 1;

            CancellationTokenSource appStartCancellationTokenSrc = null;

            while (curAttempt <= MAX_ATTEMPTS) 
            {
                try
                {
                    var appStartTimeout = timeout.HasValue ? timeout.Value : TimeSpan.FromMinutes(5);

                    if (cancellationToken != default)
                    {
                        appStartCancellationTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    }
                    else
                    {
                        appStartCancellationTokenSrc = new CancellationTokenSource();
                    }

                    appStartCancellationTokenSrc.CancelAfter(appStartTimeout);

                    m_UserLogger.WriteLine($"Starting host application: {versionInfo.DisplayName}");

                    var app = m_AppProvider.StartApplication(versionInfo,
                        opts, appStartCancellationTokenSrc.Token);

                    TryAddProcessToJob(app);

                    return app;
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new JobCancelledException();
                    }
                    
                    m_UserLogger.WriteLine($"Failed to start application from attempt {curAttempt}");

                    curAttempt++;
                }
                finally 
                {
                    appStartCancellationTokenSrc?.Dispose();
                }
            }

            throw new UserMessageException("Failed to start application. Operation is aborted");
        }

        private void TryAddProcessToJob(IXApplication app)
        {
            try
            {
                m_JobMgr.AddProcess(app.Process);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }

        private bool AttemptProcessFile(ref IXApplication app, ref Process appPrc, 
            JobItemFile file, BatchJob opts, CancellationToken cancellationToken = default)
        {
            file.Status = JobItemStatus_e.InProgress;

            int curAttempt = 1;

            var macrosStack = new List<JobItemMacro>(file.Macros);

            TimeSpan? timeout = null;
            
            if (opts.Timeout > 0)
            {
                timeout = TimeSpan.FromSeconds(opts.Timeout);
            }

            while (curAttempt <= MAX_ATTEMPTS)
            {   
                if (app == null || !IsAppAlive(app, appPrc) && !cancellationToken.IsCancellationRequested)
                {
                    TryShutDownApplication(appPrc);

                    app = AttemptStartApplication(opts.Version, opts.StartupOptions, cancellationToken, timeout);
                    
                    appPrc = app.Process;
                }

                IXDocument doc = null;
                CancellationTokenSource cancellationTokenSrc = null;

                CancellationToken timeoutCancellationToken = default;

                try
                {
                    if (timeout.HasValue) 
                    {
                        cancellationTokenSrc = new CancellationTokenSource(timeout.Value);
                        timeoutCancellationToken = cancellationTokenSrc.Token;

                        var thisAppPrc = appPrc;
                        
                        timeoutCancellationToken.Register(() =>
                        {
                            TryShutDownApplication(thisAppPrc);
                        });
                    }

                    if (cancellationToken.IsCancellationRequested) 
                    {
                        throw new JobCancelledException();
                    }

                    var fileProcessStartTime = DateTime.Now;
                    m_UserLogger.WriteLine($"Started processing file {file.FilePath}");
                    
                    doc = app.Documents.FirstOrDefault(d => string.Equals(d.Path, file.FilePath));

                    if (doc == null)
                    {
                        doc = app.Documents.PreCreate<IXDocument>();

                        doc.Path = file.FilePath;

                        var state = DocumentState_e.Default;

                        if (opts.OpenFileOptions.HasFlag(OpenFileOptions_e.Silent)) 
                        {
                            state |= DocumentState_e.Silent;
                        }

                        if (opts.OpenFileOptions.HasFlag(OpenFileOptions_e.ReadOnly)) 
                        {
                            state |= DocumentState_e.ReadOnly;
                        }

                        if (opts.OpenFileOptions.HasFlag(OpenFileOptions_e.Rapid)) 
                        {
                            state |= DocumentState_e.Rapid;
                        }

                        if (opts.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
                        {
                            state |= DocumentState_e.Hidden;
                        }

                        doc.Commit(cancellationToken);
                    }

                    app.Documents.Active = doc;

                    AttempRunMacros(app, doc, macrosStack, cancellationToken);

                    file.Status = macrosStack.Any() ? JobItemStatus_e.Warning : JobItemStatus_e.Succeeded;
                    m_UserLogger.WriteLine($"Processing file '{file.FilePath}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

                    return true;
                }
                catch(Exception ex)
                {
                    var errDesc = "";

                    if (cancellationToken.IsCancellationRequested) 
                    {
                        throw new JobCancelledException();
                    }

                    if (timeoutCancellationToken.IsCancellationRequested)
                    {
                        errDesc = "Timeout processing document";
                    }

                    else
                    {
                        if (ex is OpenDocumentFailedException)
                        {
                            errDesc = $"Failed to open document {file.FilePath}: {(ex as OpenDocumentFailedException).Message}";
                        }
                        else
                        {
                            errDesc = "Failed to process document";
                        }
                    }

                    m_UserLogger.WriteLine($"{errDesc}. Attempt: {curAttempt}");
                    curAttempt++;
                }
                finally 
                {
                    cancellationTokenSrc?.Dispose();
                    TryCloseDocument(doc);
                }
            }

            file.Status = JobItemStatus_e.Failed;
            m_UserLogger.WriteLine($"Processing file '{file.FilePath}' failed");

            return false;
        }

        private void AttempRunMacros(IXApplication app, IXDocument doc, List<JobItemMacro> macrosStack, CancellationToken cancellationToken)
        {
            while (macrosStack.Any())
            {
                var macroItem = macrosStack.First();

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new JobCancelledException();
                    }

                    macroItem.Status = JobItemStatus_e.InProgress;
                    m_UserLogger.WriteLine($"Running '{macroItem.FilePath}' macro");

                    m_MacroRunnerSvc.RunMacro(app, macroItem.Macro.FilePath, null,
                        XCad.Enums.MacroRunOptions_e.UnloadAfterRun,
                        macroItem.Macro.Arguments, doc);

                    macroItem.Status = JobItemStatus_e.Succeeded;
                }
                catch (JobCancelledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    macroItem.Status = JobItemStatus_e.Failed;

                    string errorDesc;

                    if (ex is MacroRunFailedException)
                    {
                        errorDesc = (ex as MacroRunFailedException).Message;
                    }
                    else
                    {
                        errorDesc = "Unknown error";
                    }

                    m_UserLogger.WriteLine($"Failed to run macro '{macroItem}': {errorDesc}");

                    if (!IsDocAlive(doc))
                    {
                        throw new UserMessageException("Document has been disconnected");
                    }
                }
                
                macrosStack.RemoveAt(0);
            }
        }

        private bool IsAppAlive(IXApplication app, Process prc) 
        {
            if (prc == null || prc.HasExited || !prc.Responding)
            {
                m_UserLogger.WriteLine("Application host is not responding or exited");
                return false;
            }
            else 
            {
                try
                {
                    var testWnd = app.WindowHandle;
                    return true;
                }
                catch 
                {
                    m_UserLogger.WriteLine("Application host is corrupted");
                    return false;
                }
            }
        }

        private bool IsDocAlive(IXDocument doc) 
        {
            try
            {
                var testTitle = doc.Title;
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public void Dispose()
        {
            m_AppProvider.Dispose();
        }
    }
}
