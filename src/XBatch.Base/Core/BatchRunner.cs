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
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    internal class JobItem : IJobItem
    {
        public event Action<IJobItem, JobItemStatus_e> StatusChanged;

        public string DisplayName { get; protected set; }
        
        internal string FilePath { get; }

        public JobItemStatus_e Status 
        {
            get => m_Status;
            set 
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        private JobItemStatus_e m_Status;

        internal JobItem(string filePath) 
        {
            FilePath = filePath;
            m_Status = JobItemStatus_e.AwaitingProcessing;
        }
    }

    internal class JobItemMacro : JobItem, IJobItemOperation
    {
        internal JobItemMacro(string filePath) : base(filePath)
        {
            DisplayName = Path.GetFileNameWithoutExtension(filePath);
        }
    }

    internal class JobItemFile : JobItem, IJobItemFile
    {
        internal JobItemFile(string filePath, JobItemMacro[] macros) : base(filePath)
        {
            DisplayName = Path.GetFileName(filePath);
            Macros = macros;
        }

        IEnumerable<IJobItemOperation> IJobItemFile.Operations => Macros;

        public JobItemMacro[] Macros { get; }
    }

    public class BatchRunner : IDisposable
    {
        private const int MAX_ATTEMPTS = 3;

        private readonly TextWriter m_Logger;
        private readonly IProgressHandler m_ProgressHandler;
        private readonly IApplicationProvider m_AppProvider;

        public BatchRunner(IApplicationProvider appProvider, TextWriter logger, IProgressHandler progressHandler)
        {
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
            m_AppProvider = appProvider;   
        }

        public async Task<bool> BatchRun(BatchJob opts, CancellationToken cancellationToken = default)
        {
            m_Logger.WriteLine($"Batch macro running started");

            var batchStartTime = DateTime.Now;
            
            var allFiles = PrepareJobScope(opts.Input, opts.Filters, opts.Macros).ToArray();

            m_Logger.WriteLine($"Running batch processing for {allFiles.Length} file(s)");

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
                    m_Logger.WriteLine($"Cancelled by the user");
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

                        if (opts.BatchSize > 0 && curBatchSize >= opts.BatchSize) 
                        {
                            m_Logger.WriteLine("Closing application as batch size reached the limit");
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

            m_Logger.WriteLine($"Batch running completed in {duration.ToString(@"hh\:mm\:ss")}");

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

        private IEnumerable<JobItemFile> PrepareJobScope(IEnumerable<string> inputs,  string[] filters, IEnumerable<string> macros) 
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
                                m_Logger.WriteLine($"Skipping file '{file}'");
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
                        m_Logger.WriteLine("Closing host application");
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

                    m_Logger.WriteLine($"Starting host application: {versionInfo.DisplayName}");

                    var app = m_AppProvider.StartApplication(versionInfo,
                        opts, appStartCancellationTokenSrc.Token);

                    return app;
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new JobCancelledException();
                    }
                    
                    m_Logger.WriteLine($"Failed to start application from attempt {curAttempt}");

                    curAttempt++;
                }
                finally 
                {
                    appStartCancellationTokenSrc?.Dispose();
                }
            }

            throw new UserMessageException("Failed to start application. Operation is aborted");
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
                if (app == null || !IsAppAlive(app, appPrc))
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

                    var fileProcessStartTime = DateTime.Now;
                    m_Logger.WriteLine($"Started processing file {file.FilePath}");
                    
                    doc = app.Documents.FirstOrDefault(d => string.Equals(d.Path, file.FilePath));

                    if (doc == null)
                    {
                        var openArgs = new DocumentOpenArgs()
                        {
                            Path = file.FilePath,
                            Silent = opts.OpenFileOptions.HasFlag(OpenFileOptions_e.Silent),
                            ReadOnly = opts.OpenFileOptions.HasFlag(OpenFileOptions_e.ReadOnly),
                            Rapid = opts.OpenFileOptions.HasFlag(OpenFileOptions_e.Rapid)
                        };

                        doc = app.Documents.Open(openArgs);
                    }

                    app.Documents.Active = doc;

                    AttempRunMacros(app, doc, macrosStack);

                    file.Status = macrosStack.Any() ? JobItemStatus_e.Warning : JobItemStatus_e.Succeeded;
                    m_Logger.WriteLine($"Processing file '{file.FilePath}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

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

                    m_Logger.WriteLine($"{errDesc}. Attempt: {curAttempt}");
                    curAttempt++;
                }
                finally 
                {
                    cancellationTokenSrc?.Dispose();
                    TryCloseDocument(doc);
                }
            }

            file.Status = JobItemStatus_e.Failed;
            m_Logger.WriteLine($"Processing file '{file.FilePath}' failed");

            return false;
        }

        private void AttempRunMacros(IXApplication app, IXDocument doc, List<JobItemMacro> macrosStack)
        {
            while (macrosStack.Any())
            {
                var macroItem = macrosStack.First();

                try
                {
                    macroItem.Status = JobItemStatus_e.InProgress;
                    m_Logger.WriteLine($"Running '{macroItem.FilePath}' macro");

                    var macro = app.OpenMacro(macroItem.FilePath);
                    macro.Run();
                    macroItem.Status = JobItemStatus_e.Succeeded;
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

                    m_Logger.WriteLine($"Failed to run macro '{macroItem}': {errorDesc}");

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
                m_Logger.WriteLine("Application host is not responding or exited");
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
                    m_Logger.WriteLine("Application host is corrupted");
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
        }
    }
}
