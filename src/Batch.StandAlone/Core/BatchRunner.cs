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
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
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
    public class BatchJobContext 
    {
        public IXApplication CurrentApplication { get; set; }
        public Process CurrentApplicationProcess { get; set; }
        public IXDocument CurrentDocument { get; set; }
        
        public bool? ForbidSaving { get; set; }

        public BatchJob Job { get; set; }

        public JobItemFile CurrentFile { get; set; }
        public JobItemMacro CurrentMacro { get; set; }

        public BatchJobContext() 
        {
        }
    }

    public class BatchRunner : IDisposable
    {
        private readonly TextWriter m_UserLogger;
        private readonly IProgressHandler m_ProgressHandler;
        private readonly IApplicationProvider m_AppProvider;
        private readonly IMacroRunnerExService m_MacroRunnerSvc;

        private readonly IXLogger m_Logger;
        private readonly IJobManager m_JobMgr;

        private readonly Func<TimeSpan?, IResilientWorker<BatchJobContext>> m_WorkerFact;

        private readonly IPopupKiller m_PopupKiller;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        public BatchRunner(IApplicationProvider appProvider, 
            TextWriter userLogger, IProgressHandler progressHandler,
            IBatchApplicationProxy batchAppProxy,
            IJobManager jobMgr, IXLogger logger,
            Func<TimeSpan?, IResilientWorker<BatchJobContext>> workerFact, IPopupKiller popupKiller)
        {
            m_UserLogger = userLogger;
            m_ProgressHandler = progressHandler;
            m_MacroRunnerSvc = appProvider.MacroRunnerService;
            m_AppProvider = appProvider;
            m_WorkerFact = workerFact;
            m_BatchAppProxy = batchAppProxy;

            m_PopupKiller = popupKiller;
            m_PopupKiller.PopupNotClosed += OnPopupNotClosed;

            m_Logger = logger;

            m_JobMgr = jobMgr;
        }

        private void OnPopupNotClosed(Process prc)
        {
            m_UserLogger.WriteLine("Failed to close the blocking popup window");
            TryShutDownApplication(prc);
        }

        public async Task<bool> BatchRun(BatchJob opts, CancellationToken cancellationToken = default)
        {
            m_UserLogger.WriteLine($"Batch macro running started");

            var batchStartTime = DateTime.Now;
            
            TimeSpan? timeout = null;

            if (opts.Timeout > 0)
            {
                timeout = TimeSpan.FromSeconds(opts.Timeout);
            }

            var worker = m_WorkerFact.Invoke(timeout);

            worker.Retry += OnRetry;
            worker.Timeout += OnTimeout;
            
            var context = new BatchJobContext()
            {
                Job = opts
            };

            var jobResult = false;

            try
            {
                await Task.Run(() =>
                {
                    m_UserLogger.WriteLine($"Collecting files for processing");

                    var app = EnsureApplication(context, cancellationToken);

                    var allFiles = PrepareJobScope(app, opts.Input, opts.Filters, opts.Macros);

                    m_UserLogger.WriteLine($"Running batch processing for {allFiles.Length} file(s)");

                    m_ProgressHandler.SetJobScope(allFiles, batchStartTime);

                    if (!allFiles.Any())
                    {
                        throw new UserException("Empty job. No files matching specified filter");
                    }

                    var curBatchSize = 0;

                    for (int i = 0; i < allFiles.Length; i++)
                    {
                        var curAppPrc = context.CurrentApplicationProcess?.Id;

                        context.CurrentFile = allFiles[i];
                        var res = TryProcessFile(context, worker, cancellationToken);
                        
                        TryCloseDocument(context.CurrentDocument);

                        context.CurrentDocument = null;

                        m_ProgressHandler?.ReportProgress(context.CurrentFile, res);

                        if (!res && !opts.ContinueOnError)
                        {
                            throw new UserException("Cancelling the job. Set 'Continue On Error' option to continue job if file failed");
                        }

                        if (context.CurrentApplicationProcess?.Id != curAppPrc)
                        {
                            curBatchSize = 1;
                        }
                        else
                        {
                            curBatchSize++;
                        }

                        if (opts.BatchSize > 0 && curBatchSize >= opts.BatchSize) 
                        {
                            m_UserLogger.WriteLine("Closing application as batch size reached the limit");
                            TryShutDownApplication(context.CurrentApplicationProcess);
                            curBatchSize = 0;
                        }
                    }
                }).ConfigureAwait(false);
                jobResult = true;
            }
            catch (OperationCanceledException)
            {
                throw new JobCancelledException();
            }
            finally
            {
                TryShutDownApplication(context.CurrentApplicationProcess);
            }

            var duration = DateTime.Now.Subtract(batchStartTime);

            m_ProgressHandler.ReportCompleted(duration);

            m_UserLogger.WriteLine($"Batch running completed in {duration.ToString(@"hh\:mm\:ss")}");

            return jobResult;
        }

        private void OnRetry(Exception err, int retry, BatchJobContext context)
        {
            m_UserLogger.WriteLine($"Failed to run macro: {err.ParseUserError(out _)}. Retrying (retry #{retry})...");

            ProcessError(err, context);

            m_Logger.Log(err);
        }

        private void OnTimeout(BatchJobContext context)
        {
            m_UserLogger.WriteLine("Operation timed out");
            TryShutDownApplication(context.CurrentApplicationProcess);
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

        private JobItemFile[] PrepareJobScope(IXApplication app,
            IEnumerable<string> inputs,  string[] filters, IEnumerable<MacroData> macros) 
        {
            var inputFiles = new List<string>();

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
                                inputFiles.Add(file);
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
                    inputFiles.Add(input);
                }
                else
                {
                    throw new Exception("Specify input file or directory");
                }
            }
            
            m_BatchAppProxy.ProcessInput(app, inputFiles);

            return inputFiles
                .Select(f => new JobItemFile(f, macros.Select(m => new JobItemMacro(m)).ToArray()))
                .ToArray();
        }

        private void TryCloseDocument(IXDocument doc)
        {
            if (doc != null && doc.IsCommitted)
            {
                try
                {
                    m_UserLogger.WriteLine($"Closing '{doc.Path}'");

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

                if (m_PopupKiller.IsStarted)
                {
                    m_PopupKiller.Stop();
                }
            }
            catch 
            {
            }
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

        private bool TryProcessFile(BatchJobContext context,
            IResilientWorker<BatchJobContext> worker,
            CancellationToken cancellationToken)
        {
            var fileProcessStartTime = DateTime.Now;
            m_UserLogger.WriteLine($"Started processing file {context.CurrentFile.FilePath}");

            context.ForbidSaving = null;
            context.CurrentFile.Status = JobItemStatus_e.InProgress;

            foreach (var macro in context.CurrentFile.Macros)
            {
                context.CurrentMacro = macro;

                try
                {
                    worker.DoWork(RunMacro, context, cancellationToken);
                    macro.Status = JobItemStatus_e.Succeeded;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ProcessError(ex, context);

                    m_UserLogger.WriteLine(ex.ParseUserError(out _));
                    m_Logger.Log(ex);
                }
            }

            if (context.CurrentFile.Macros.All(m => m.Status == JobItemStatus_e.Succeeded))
            {
                context.CurrentFile.Status = JobItemStatus_e.Succeeded;
            }
            else
            {
                context.CurrentFile.Status = context.CurrentFile.Macros.Any(m => m.Status == JobItemStatus_e.Succeeded) ? JobItemStatus_e.Warning : JobItemStatus_e.Failed;
            }

            m_UserLogger.WriteLine($"Processing file '{context.CurrentFile.FilePath}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

            return context.CurrentFile.Status != JobItemStatus_e.Failed;
        }

        private void ProcessError(Exception err, BatchJobContext context) 
        {
            if (err is ICriticalException)
            {
                m_UserLogger.WriteLine("Critical error - restarting application");

                TryShutDownApplication(context.CurrentApplicationProcess);
            }
        }

        private void RunMacro(BatchJobContext context, CancellationToken cancellationToken)
        {
            EnsureApplication(context, cancellationToken);

            if (context.CurrentDocument == null || !context.CurrentDocument.IsAlive)
            {
                TryCloseDocument(context.CurrentDocument);

                context.CurrentDocument = OpenDocument(context.CurrentApplication,
                    context.CurrentFile.FilePath, context.Job.OpenFileOptions, cancellationToken, out bool forbidSaving);

                context.ForbidSaving = forbidSaving;
            }

            if (!context.Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
            {
                context.CurrentApplication.Documents.Active = context.CurrentDocument;
            }

            m_UserLogger.WriteLine($"Running '{context.CurrentMacro.FilePath}' macro");

            try
            {
                m_MacroRunnerSvc.RunMacro(context.CurrentApplication, context.CurrentMacro.FilePath, null,
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun,
                    context.CurrentMacro.Macro.Arguments, context.CurrentDocument);
            }
            catch (MacroRunFailedException ex)
            {
                if (ex is ICriticalException)
                {
                    throw;
                }
                else
                {
                    throw new UserException($"Failed to run macro '{context.CurrentMacro.DisplayName}': {ex.Message}", ex);
                }
            }

            if (context.Job.Actions.HasFlag(Actions_e.AutoSaveDocuments))
            {
                if (!context.ForbidSaving.Value)
                {
                    m_UserLogger.WriteLine("Saving the document");
                    context.CurrentDocument.Save();
                }
                else
                {
                    throw new SaveForbiddenException();
                }
            }
        }

        private IXApplication EnsureApplication(BatchJobContext context, CancellationToken cancellationToken)
        {
            if (context.CurrentApplication == null || !context.CurrentApplication.IsAlive)
            {
                TryShutDownApplication(context.CurrentApplicationProcess);

                var vers = m_AppProvider.ParseVersion(context.Job.VersionId);

                context.CurrentApplication = StartApplication(vers, context.Job.StartupOptions,
                    cancellationToken);

                context.CurrentApplicationProcess = context.CurrentApplication.Process;
            }

            return context.CurrentApplication;
        }

        private IXApplication StartApplication(IXVersion versionInfo,
            StartupOptions_e opts, CancellationToken cancellationToken)
        {
            m_UserLogger.WriteLine($"Starting host application: {versionInfo.DisplayName}");

            IXApplication app;

            try
            {
                app = m_AppProvider.StartApplication(versionInfo,
                    opts, cancellationToken);

                if (opts.HasFlag(StartupOptions_e.Silent)) 
                {
                    m_PopupKiller.Start(app.Process, TimeSpan.FromSeconds(2));
                }
            }
            catch (Exception ex)
            {
                throw new UserException("Failed to start host application", ex);
            }

            TryAddProcessToJob(app);

            return app;
        }

        private IXDocument OpenDocument(IXApplication app, 
            string filePath, OpenFileOptions_e opts, CancellationToken cancellationToken, out bool forbidSaving) 
        {
            var doc = app.Documents.FirstOrDefault(d => string.Equals(d.Path, filePath));

            if (doc == null)
            {
                doc = app.Documents.PreCreate<IXDocument>();

                doc.Path = filePath;

                var state = DocumentState_e.Default;

                if (opts.HasFlag(OpenFileOptions_e.Silent))
                {
                    state |= DocumentState_e.Silent;
                }

                if (opts.HasFlag(OpenFileOptions_e.ReadOnly))
                {
                    state |= DocumentState_e.ReadOnly;
                }

                if (opts.HasFlag(OpenFileOptions_e.Rapid))
                {
                    state |= DocumentState_e.Rapid;
                }

                if (opts.HasFlag(OpenFileOptions_e.Invisible))
                {
                    state |= DocumentState_e.Hidden;
                }

                forbidSaving = NeedForbidSaving(app, doc, opts);

                if (forbidSaving)
                {
                    if (!state.HasFlag(DocumentState_e.ReadOnly))
                    {
                        m_UserLogger.WriteLine($"Setting the readonly flag to {filePath} to prevent upgrade of the file");
                        state |= DocumentState_e.ReadOnly;
                    }
                }

                doc.State = state;

                try
                {
                    m_UserLogger.WriteLine($"Opening '{filePath}'");

                    doc.Commit(cancellationToken);
                }
                catch (OpenDocumentFailedException ex)
                {
                    throw new UserException($"Failed to open document {filePath}: {(ex as OpenDocumentFailedException).Message}", ex);
                }
            }
            else 
            {
                forbidSaving = NeedForbidSaving(app, doc, opts);

                if (forbidSaving && !doc.State.HasFlag(DocumentState_e.ReadOnly)) 
                {
                    TryCloseDocument(doc);
                    throw new UserException("Document is opened with write access, but saving is forbidden");
                }
            }

            return doc;
        }

        private bool NeedForbidSaving(IXApplication app, IXDocument doc, OpenFileOptions_e opts) 
        {            
            if (opts.HasFlag(OpenFileOptions_e.ForbidUpgrade))
            {
                try
                {
                    if (app.Version.Compare(doc.Version) == VersionEquality_e.Newer)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new UserException("Failed to extract version of the file", ex);
                }
            }

            return false;
        }

        public void Dispose()
        {
            m_AppProvider.Dispose();
        }
    }
}
