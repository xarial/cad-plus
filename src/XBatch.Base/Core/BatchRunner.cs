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
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchRunner : IDisposable
    {
        private const int MAX_ATTEMPTS = 3;

        private readonly TextWriter m_Logger;
        private readonly IProgress<double> m_ProgressHandler;
        private readonly IApplicationProvider m_AppProvider;

        public BatchRunner(IApplicationProvider appProvider, TextWriter logger, IProgress<double> progressHandler)
        {
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
            m_AppProvider = appProvider;   
        }

        public async Task<bool> BatchRun(BatchRunnerOptions opts, CancellationToken cancellationToken = default)
        {
            m_Logger.WriteLine($"Batch macro running started");

            var batchStartTime = DateTime.Now;
            
            var allFiles = SelectAllFiles(opts.Input, opts.Filter).ToArray();

            m_Logger.WriteLine($"Running batch processing for {allFiles.Length} file(s)");
            
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
                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        var res = AttemptProcessFile(ref app, ref appPrc, allFiles[i], opts, cancellationToken);
                        m_ProgressHandler?.Report((i + 1) / (double)allFiles.Length);
                                                
                        if (!res && !opts.ContinueOnError) 
                        {
                            throw new UserMessageException("Cancelling the job. Set 'Continue On Error' option to continue job if file failed");
                        }
                    }
                });
                jobResult = true;
            }
            finally
            {
                TryShutDownApplication(appPrc);
            }

            m_Logger.WriteLine($"Batch running completed in {DateTime.Now.Subtract(batchStartTime).ToString(@"hh\:mm\:ss")}");

            return jobResult;
        }

        private IEnumerable<string> SelectAllFiles(IEnumerable<string> inputs, string filter) 
        {
            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    foreach (var file in Directory.EnumerateFiles(input, filter, SearchOption.AllDirectories))
                    {
                        yield return file;
                    }
                }
                else if (File.Exists(input))
                {
                    yield return input;
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

                    m_Logger.WriteLine($"Starting host application");

                    var app = m_AppProvider.StartApplication(versionInfo,
                        opts, appStartCancellationTokenSrc.Token);

                    return app;
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new UserMessageException("Cancelled by the user");
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
            string filePath, BatchRunnerOptions opts, CancellationToken cancellationToken = default)
        {
            int curAttempt = 1;

            var macrosStack = new List<string>(opts.Macros);

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
                    m_Logger.WriteLine($"Started processing file {filePath}");
                    
                    doc = app.Documents.FirstOrDefault(d => string.Equals(d.Path, filePath));

                    if (doc == null)
                    {
                        doc = app.Documents.Open(new DocumentOpenArgs()
                        {
                            Path = filePath,
                            Silent = true
                        });
                    }

                    app.Documents.Active = doc;

                    AttempRunMacros(app, doc, macrosStack);

                    m_Logger.WriteLine($"Processing file '{filePath}' successfully completed. Failed macros: {macrosStack.Count}. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

                    return true;
                }
                catch(Exception ex)
                {
                    var errDesc = "";

                    if (cancellationToken.IsCancellationRequested) 
                    {
                        throw new UserMessageException("Cancelled by the user");
                    }

                    if (timeoutCancellationToken.IsCancellationRequested)
                    {
                        errDesc = "Timeout processing document";
                    }

                    else
                    {
                        if (ex is OpenDocumentFailedException)
                        {
                            errDesc = $"Failed to open document {filePath}: {(ex as OpenDocumentFailedException).Message}";
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

            m_Logger.WriteLine($"Processing file '{filePath}' failed");
            return false;
        }

        private void AttempRunMacros(IXApplication app, IXDocument doc, List<string> macrosStack)
        {
            while (macrosStack.Any())
            {
                var macroPath = macrosStack.First();

                try
                {
                    m_Logger.WriteLine($"Running '{macroPath}' macro");

                    var macro = app.OpenMacro(macroPath);
                    macro.Run();
                }
                catch (Exception ex)
                {
                    string errorDesc;

                    if (ex is MacroRunFailedException)
                    {
                        errorDesc = (ex as MacroRunFailedException).Message;
                    }
                    else
                    {
                        errorDesc = "Unknown error";
                    }

                    m_Logger.WriteLine($"Failed to run macro '{macroPath}': {errorDesc}");

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
            if (prc.HasExited || !prc.Responding)
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
