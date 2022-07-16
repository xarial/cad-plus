//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.XCad;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using System.Diagnostics;
using Xarial.XCad.Documents;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.StandAlone;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XToolkit;
using Xarial.XCad.Documents.Enums;
using System.Linq;
using Xarial.XCad.Exceptions;
using Xarial.CadPlus.Batch.StandAlone.Exceptions;
using Xarial.CadPlus.Plus.Shared.Extensions;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public class BatchJobContext
    {
        public IXApplication CurrentApplication { get; set; }
        public Process CurrentApplicationProcess { get; set; }
        public IXDocument CurrentDocument { get; set; }

        public bool? ForbidSaving { get; set; }

        public BatchJob Job { get; set; }

        public JobItemDocument CurrentJobItem { get; set; }
        public JobItemMacro CurrentMacro { get; set; }

        public BatchJobContext()
        {
        }
    }

    public class BatchRunJobExecutor : IBatchRunJobExecutor
    {
        private const double POPUP_KILLER_PING_SECS = 2;

        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<IJobItem, double, bool> ProgressChanged;
        public event Action<string> StatusChanged;
        public event Action<string> Log;
        public event Action<TimeSpan> JobCompleted;
        
        private bool m_IsExecuted;
        private bool m_IsDisposed;

        private readonly IBatchApplicationProxy m_BatchAppProxy;
        private readonly IJobManager m_JobMgr;
        private readonly IXLogger m_Logger;
        private readonly IResilientWorker<BatchJobContext> m_Worker;
        private readonly IPopupKiller m_PopupKiller;
        private readonly ICadApplicationInstanceProvider m_AppProvider;
        private readonly IMacroExecutor m_MacroRunnerSvc;
        private readonly BatchJob m_Job;

        private readonly BatchJobContext m_CurrentContext;

        private readonly object m_Lock;

        public BatchRunJobExecutor(BatchJob job, ICadApplicationInstanceProvider appProvider,
            IBatchApplicationProxy batchAppProxy,
            IJobManager jobMgr, IXLogger logger,
            IResilientWorker<BatchJobContext> worker, IPopupKiller popupKiller)
        {
            m_Job = job;
            m_AppProvider = appProvider;
            m_MacroRunnerSvc = m_AppProvider.MacroRunnerService;

            m_Lock = new object();

            m_Worker = worker;

            m_Worker.Retry += OnRetry;
            m_Worker.Timeout += OnTimeout;

            m_BatchAppProxy = batchAppProxy;

            m_PopupKiller = popupKiller;
            m_PopupKiller.ShouldClosePopup += OnShouldClosePopup;
            m_PopupKiller.PopupNotClosed += OnPopupNotClosed;

            m_Logger = logger;

            m_JobMgr = jobMgr;

            m_CurrentContext = new BatchJobContext()
            {
                Job = m_Job
            };

            m_IsExecuted = false;
        }

        public bool Execute(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken).Result;

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                m_Logger.Log("Trying to cancel batch runner", LoggerMessageSeverity_e.Debug);

                //calling to immediately cancel the operation by killing the cad application process
                TryShutDownApplication(m_CurrentContext);
            });

            try
            {
                return await BatchRunAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log?.Invoke(ex.ParseUserError());
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        private async Task<bool> BatchRunAsync(CancellationToken cancellationToken = default)
        {
            if (!m_IsExecuted)
            {
                m_IsExecuted = true;

                Log?.Invoke($"Batch macro running started");

                var batchStartTime = DateTime.Now;

                TimeSpan? timeout = null;

                if (m_Job.Timeout > 0)
                {
                    timeout = TimeSpan.FromSeconds(m_Job.Timeout);
                }

                var jobResult = false;

                try
                {
                    await TaskEx.Run(() =>
                    {
                        Log?.Invoke($"Collecting files for processing");

                        var app = EnsureApplication(m_CurrentContext, cancellationToken);

                        var allFiles = PrepareJobScope(app, m_Job.Input, m_Job.Filters, m_Job.TopLevelFilesOnly, m_Job.Macros);

                        Log?.Invoke($"Running batch processing for {allFiles.Length} file(s)");

                        JobSet?.Invoke(allFiles, batchStartTime);

                        if (!allFiles.Any())
                        {
                            throw new UserException("Empty job. No files matching specified filter");
                        }

                        var curBatchSize = 0;

                        for (int i = 0; i < allFiles.Length; i++)
                        {
                            StatusChanged?.Invoke($"Processing {allFiles[i].FilePath}");

                            var curAppPrc = m_CurrentContext.CurrentApplicationProcess?.Id;

                            m_CurrentContext.CurrentJobItem = allFiles[i];
                            var res = TryProcessFile(m_CurrentContext, m_Worker, cancellationToken);

                            TryCloseDocument(m_CurrentContext.CurrentDocument);

                            m_CurrentContext.CurrentDocument = null;

                            ProgressChanged?.Invoke(m_CurrentContext.CurrentJobItem, (double)(i + 1) / (double)allFiles.Length, res);

                            if (!res && !m_Job.ContinueOnError)
                            {
                                throw new UserException("Cancelling the job. Set 'Continue On Error' option to continue job if file failed");
                            }

                            if (m_CurrentContext.CurrentApplicationProcess?.Id != curAppPrc)
                            {
                                curBatchSize = 1;
                            }
                            else
                            {
                                curBatchSize++;
                            }

                            if (m_Job.BatchSize > 0 && curBatchSize >= m_Job.BatchSize)
                            {
                                Log?.Invoke("Closing application as batch size reached the limit");
                                TryShutDownApplication(m_CurrentContext);
                                curBatchSize = 0;
                            }
                        }
                    }, new StaTaskScheduler(m_Logger)).ConfigureAwait(false);
                    jobResult = true;
                }
                catch (OperationCanceledException)
                {
                    throw new JobCancelledException();
                }
                finally
                {
                    TryShutDownApplication(m_CurrentContext);
                }

                var duration = DateTime.Now.Subtract(batchStartTime);

                JobCompleted?.Invoke(duration);

                Log?.Invoke($"Batch running completed in {duration.ToString(@"hh\:mm\:ss")}");

                return jobResult;
            }
            else
            {
                throw new Exception("Job was already executed. This is a transient service and can onlyt be executed once");
            }
        }

        private void OnPopupNotClosed(Process prc, IntPtr hWnd)
        {
            //VBA error popup cannot be closed automatically
            TryHandleVbaExceptionPopup(hWnd);
        }

        private void OnShouldClosePopup(Process prc, IntPtr hWnd, ref bool close)
        {
            if (m_CurrentContext.Job.StartupOptions.HasFlag(StartupOptions_e.Silent))
            {
                //attempt to close all popups in the silent mode
                close = true;
            }
            else
            {
                close = false;
                //only close VBA error popup
                TryHandleVbaExceptionPopup(hWnd);
            }
        }

        private void TryHandleVbaExceptionPopup(IntPtr hWnd)
        {
            using (var vbaErrPopup = new VbaErrorPopup(hWnd))
            {
                if (vbaErrPopup.IsVbaErrorPopup)
                {
                    var curMacro = m_CurrentContext.CurrentMacro;

                    if (curMacro != null)
                    {
                        curMacro.InternalMacroException = new VbaMacroException(vbaErrPopup.ErrorText);
                    }

                    m_Logger.Log($"Closing VBA Error popup window: {hWnd}", LoggerMessageSeverity_e.Debug);

                    vbaErrPopup.Close();
                }
                else
                {
                    m_Logger.Log($"Blocking popup window is not closed: {hWnd}", LoggerMessageSeverity_e.Debug);

                    Log?.Invoke("Failed to close the blocking popup window");
                }
            }
        }

        private void OnRetry(Exception err, int retry, BatchJobContext context)
        {
            Log?.Invoke($"Failed to run macro: {err.ParseUserError()}. Retrying (retry #{retry})...");

            ProcessError(err, context);

            m_Logger.Log(err);
        }

        private void OnTimeout(BatchJobContext context)
        {
            Log?.Invoke("Operation timed out");
            TryShutDownApplication(context);
        }

        private JobItemDocument[] PrepareJobScope(IXApplication app,
            IEnumerable<string> inputs, string[] filters, bool topLevelOnly, IEnumerable<MacroData> macros)
        {
            var inputFiles = new List<string>();

            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    var searchOpts = topLevelOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

                    foreach (var file in Directory.EnumerateFiles(input, "*.*", searchOpts))
                    {
                        if (TextUtils.MatchesAnyFilter(file, filters))
                        {
                            if (!m_AppProvider.Descriptor.IsSystemFile(file))
                            {
                                if (!inputFiles.Contains(input, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    inputFiles.Add(file);
                                }
                            }
                            else
                            {
                                Log?.Invoke($"Skipping file '{file}'");
                            }
                        }
                    }
                }
                else if (File.Exists(input))
                {
                    if (!inputFiles.Contains(input, StringComparer.CurrentCultureIgnoreCase))
                    {
                        inputFiles.Add(input);
                    }
                }
                else
                {
                    throw new UserException($"Input '{input}' does not exist");
                }
            }

            var inputDocs = inputFiles.Select(f =>
            {
                IXDocument doc;

                if (MatchesExtension(f, m_AppProvider.Descriptor.PartFileFilter.Extensions))
                {
                    doc = app.Documents.PreCreate<IXPart>();
                }
                else if (MatchesExtension(f, m_AppProvider.Descriptor.AssemblyFileFilter.Extensions))
                {
                    doc = app.Documents.PreCreate<IXAssembly>();
                }
                else if (MatchesExtension(f, m_AppProvider.Descriptor.DrawingFileFilter.Extensions))
                {
                    doc = app.Documents.PreCreate<IXDrawing>();
                }
                else
                {
                    doc = app.Documents.PreCreate<IXDocument>();
                }

                doc.Path = f;

                return doc;
            }).ToList();

            m_BatchAppProxy.ProcessInput(app, m_AppProvider, inputDocs);

            return inputDocs
                .Select(d => new JobItemDocument(d, macros.Select(m => new JobItemMacro(m)).ToArray()))
                .ToArray();
        }

        private bool MatchesExtension(string path, string[] exts)
        {
            try
            {
                var ext = Path.GetExtension(path);
                return exts.Any(e => Path.GetExtension(e).Equals(ext, StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private void TryCloseDocument(IXDocument doc)
        {
            if (doc != null && doc.IsCommitted)
            {
                try
                {
                    Log?.Invoke($"Closing '{doc.Path}'");

                    doc.Close();
                }
                catch
                {
                }
            }
        }

        private void TryShutDownApplication(BatchJobContext context)
        {
            lock (m_Lock)
            {
                if (context != null)
                {
                    //NOTE: killing the process first so it is terminated immediately as disposing may be delayed if executed on other thread
                    try
                    {
                        var appPrc = context.CurrentApplicationProcess;

                        if (appPrc != null)
                        {
                            if (!appPrc.HasExited)
                            {
                                m_Logger.Log($"Trying to shut down IXApplication process", LoggerMessageSeverity_e.Debug);

                                Log?.Invoke("Closing host application");
                                appPrc.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log($"Failed to shut down IXApplication process", LoggerMessageSeverity_e.Debug);
                        m_Logger.Log(ex);
                    }
                    finally
                    {
                        context.CurrentApplicationProcess = null;
                    }

                    try
                    {
                        var app = context.CurrentApplication;

                        if (app is IDisposable)
                        {
                            m_Logger.Log($"Trying to dispose pointer to IXApplication", LoggerMessageSeverity_e.Debug);
                            ((IDisposable)app).Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log($"Failed to dispose pointer to IXApplication", LoggerMessageSeverity_e.Debug);
                        m_Logger.Log(ex);
                    }
                    finally
                    {
                        context.CurrentApplication = null;
                    }
                }

                try
                {
                    if (m_PopupKiller != null && m_PopupKiller.IsStarted)
                    {
                        m_Logger.Log($"Trying to kill the popup killer", LoggerMessageSeverity_e.Debug);

                        m_PopupKiller.Stop();
                    }
                }
                catch
                {
                }
            }
        }

        private void TryAddProcessToJob(Process prc)
        {
            try
            {
                m_JobMgr.AddProcess(prc);
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
            Log?.Invoke($"Started processing file {context.CurrentJobItem.FilePath}");

            context.ForbidSaving = null;
            context.CurrentJobItem.Status = JobItemStatus_e.InProgress;

            foreach (var macro in context.CurrentJobItem.Macros)
            {
                context.CurrentMacro = macro;

                try
                {
                    worker.DoWork(RunMacro, context, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ProcessError(ex, context);

                    if (context.CurrentDocument == null)
                    {
                        context.CurrentJobItem.ReportError(ex);
                    }
                    else
                    {
                        context.CurrentMacro.ReportError(ex);
                    }

                    Log?.Invoke(ex.ParseUserError(out _));
                    m_Logger.Log(ex);
                }
            }

            if (context.CurrentJobItem.Macros.All(m => m.Status == JobItemStatus_e.Succeeded))
            {
                context.CurrentJobItem.Status = JobItemStatus_e.Succeeded;
            }
            else
            {
                context.CurrentJobItem.Status = context.CurrentJobItem.Macros.Any(m => m.Status == JobItemStatus_e.Succeeded) ? JobItemStatus_e.Warning : JobItemStatus_e.Failed;
            }

            Log?.Invoke($"Processing file '{context.CurrentJobItem.FilePath}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

            return context.CurrentJobItem.Status != JobItemStatus_e.Failed;
        }

        private void ProcessError(Exception err, BatchJobContext context)
        {
            if (err is ICriticalException)
            {
                Log?.Invoke("Critical error - restarting application");

                TryShutDownApplication(context);
            }
        }

        private void RunMacro(BatchJobContext context, CancellationToken cancellationToken)
        {
            EnsureApplication(context, cancellationToken);

            if (context.CurrentDocument == null || !context.CurrentDocument.IsAlive)
            {
                TryCloseDocument(context.CurrentDocument);

                context.CurrentDocument = EnsureDocument(context.CurrentApplication,
                    context.CurrentJobItem.Document, context.Job.OpenFileOptions, cancellationToken, out bool forbidSaving);

                context.ForbidSaving = forbidSaving;
            }

            if (!context.Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
            {
                context.CurrentApplication.Documents.Active = context.CurrentDocument;
            }

            Log?.Invoke($"Running '{context.CurrentMacro.FilePath}' macro");

            try
            {
                context.CurrentMacro.Status = JobItemStatus_e.InProgress;

                IXDocument macroDoc = null;

                if (!string.IsNullOrEmpty(context.CurrentMacro.Macro.Arguments)
                    || context.Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
                {
                    macroDoc = context.CurrentDocument;
                }

                m_MacroRunnerSvc.RunMacro(context.CurrentApplication, context.CurrentMacro.FilePath, null,
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun,
                    context.CurrentMacro.Macro.Arguments, macroDoc);

                if (context.CurrentMacro.InternalMacroException != null)
                {
                    var ex = context.CurrentMacro.InternalMacroException;
                    context.CurrentMacro.InternalMacroException = null;
                    throw ex;
                }

                if (context.Job.Actions.HasFlag(Actions_e.AutoSaveDocuments))
                {
                    if (!context.ForbidSaving.Value)
                    {
                        Log?.Invoke("Saving the document");

                        if (context.CurrentDocument.IsAlive)
                        {
                            context.CurrentDocument.Save();
                        }
                        else
                        {
                            throw new UserException("Failed to automatically save the document as it has been closed or disconnected");
                        }
                    }
                    else
                    {
                        throw new SaveForbiddenException();
                    }
                }

                context.CurrentMacro.Status = JobItemStatus_e.Succeeded;
            }
            catch (Exception ex)
            {
                context.CurrentMacro.Status = JobItemStatus_e.Failed;

                if (ex is ICriticalException)
                {
                    throw new CriticalErrorException(ex);
                }
                else if (ex is MacroRunFailedException)
                {
                    throw new UserException($"Failed to run macro '{context.CurrentMacro.DisplayName}': {ex.Message}", ex);
                }
                else if (ex is INoRetryMacroRunException)
                {
                    context.CurrentMacro.ReportError(ex);
                }
                else
                {
                    throw;
                }
            }
        }

        private IXApplication EnsureApplication(BatchJobContext context, CancellationToken cancellationToken)
        {
            if (context.CurrentApplication == null || !context.CurrentApplication.IsAlive)
            {
                TryShutDownApplication(context);

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
            Log?.Invoke($"Starting host application: {versionInfo.DisplayName}");

            IXApplication app;

            try
            {
                app = m_AppProvider.StartApplication(versionInfo,
                    opts, p => TryAddProcessToJob(p), cancellationToken);

                m_PopupKiller.Start(app.Process, TimeSpan.FromSeconds(POPUP_KILLER_PING_SECS));
            }
            catch (Exception ex)
            {
                throw new UserException("Failed to start host application", ex);
            }

            return app;
        }

        private IXDocument EnsureDocument(IXApplication app,
            IXDocument templateDoc, OpenFileOptions_e opts, CancellationToken cancellationToken, out bool forbidSaving)
        {
            IXDocument doc;

            if (templateDoc.IsCommitted && templateDoc.IsAlive)
            {
                doc = templateDoc;
            }
            else
            {
                doc = app.Documents.FirstOrDefault(d => string.Equals(d.Path, templateDoc.Path));

                if (doc == null)
                {
                    doc = app.Documents.PreCreate<IXDocument>();

                    doc.Path = templateDoc.Path;
                }
            }

            if (!doc.IsCommitted)
            {
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
                        Log?.Invoke($"Setting the readonly flag to {doc.Path} to prevent upgrade of the file");
                        state |= DocumentState_e.ReadOnly;
                    }
                }

                doc.State = state;

                Log?.Invoke($"Opening '{doc.Path}'");

                doc.Commit(cancellationToken);
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
                    if (TextUtils.MatchesAnyFilter(doc.Path, m_AppProvider.Descriptor.PartFileFilter.Extensions)
                        || TextUtils.MatchesAnyFilter(doc.Path, m_AppProvider.Descriptor.AssemblyFileFilter.Extensions)
                        || TextUtils.MatchesAnyFilter(doc.Path, m_AppProvider.Descriptor.DrawingFileFilter.Extensions))
                    {
                        if (app.Version.Compare(doc.Version) == VersionEquality_e.Newer)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new UserException("Failed to extract version of the file. This can indicate that the file is corrupted", ex);
                }
            }

            return false;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                TryShutDownApplication(m_CurrentContext);
                m_PopupKiller.Dispose();
            }
        }
    }
}
