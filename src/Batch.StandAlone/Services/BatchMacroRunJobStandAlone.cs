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
using Xarial.CadPlus.Plus.Shared.Exceptions;

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

    public interface IBatchMacroRunJobStandAlone : IAsyncBatchJob 
    {
    }

    public class BatchMacroRunJobStandAlone : IBatchMacroRunJobStandAlone
    {
        public event BatchJobStartedDelegate Started;
        public event BatchJobInitializedDelegate Initialized;
        public event BatchJobCompletedDelegate Completed;
        public event BatchJobItemProcessedDelegate ItemProcessed;
        public event BatchJobLogDelegateDelegate Log;

        private bool m_IsDisposed;

        private readonly IBatchApplicationProxy m_BatchAppProxy;
        private readonly IJobProcessManager m_JobMgr;
        private readonly IXLogger m_Logger;
        private readonly IResilientWorker<BatchJobContext> m_Worker;
        private readonly IMacroRunnerPopupHandler m_PopupHandler;
        private readonly ICadApplicationInstanceProvider m_AppProvider;
        private readonly IMacroExecutor m_MacroRunnerSvc;
        private readonly BatchJob m_Job;

        private readonly BatchJobContext m_CurrentContext;

        private readonly object m_Lock;

        private readonly ITaskRunner m_TaskRunner;

        public IReadOnlyList<IBatchJobItem> JobItems => m_JobDocuments;
        public IReadOnlyList<IBatchJobItemOperationDefinition> OperationDefinitions { get; private set; }
        public IReadOnlyList<string> LogEntries => m_LogEntries;

        public IBatchJobState State => m_State;

        private readonly List<string> m_LogEntries;

        private JobItemDocument[] m_JobDocuments;

        private BatchJobState m_State;

        public BatchMacroRunJobStandAlone(BatchJob job, ICadApplicationInstanceProvider appProvider,
            IBatchApplicationProxy batchAppProxy,
            IJobProcessManager jobMgr, IXLogger logger,
            IResilientWorker<BatchJobContext> worker, IMacroRunnerPopupHandler popupHandler, ITaskRunner taskRunner)
        {
            m_Job = job;
            m_AppProvider = appProvider;
            m_MacroRunnerSvc = m_AppProvider.MacroRunnerService;

            m_State = new BatchJobState();

            m_TaskRunner = taskRunner;

            m_LogEntries = new List<string>();

            m_Lock = new object();

            m_Worker = worker;

            m_Worker.Retry += OnRetry;
            m_Worker.Timeout += OnTimeout;

            m_BatchAppProxy = batchAppProxy;

            m_PopupHandler = popupHandler;
            m_PopupHandler.MacroUserError += OnMacroUserError;

            m_Logger = logger;

            m_JobMgr = jobMgr;

            m_CurrentContext = new BatchJobContext()
            {
                Job = m_Job
            };
        }

        public async Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                m_Logger.Log("Trying to cancel batch runner", LoggerMessageSeverity_e.Debug);

                //calling to immediately cancel the operation by killing the cad application process
                TryShutDownApplication(m_CurrentContext);
            });

            await this.HandleExecuteAsync(cancellationToken,
                t => Started?.Invoke(this, t),
                t => m_State.StartTime = t,
                InitAsync,
                () => Initialized?.Invoke(this, JobItems, OperationDefinitions),
                DoWorkAsync,
                d => Completed?.Invoke(this, d, m_State.Status),
                d => m_State.Duration = d,
                s => m_State.Status = s);
        }

        private async Task InitAsync(CancellationToken cancellationToken)
        {
            try
            {
                await m_TaskRunner.Run(() =>
                {

                    AddLogEntry($"Collecting files for processing");

                    var app = EnsureApplication(m_CurrentContext, cancellationToken);

                    m_JobDocuments = PrepareJobScope(app, m_Job.Input, m_Job.Filters, m_Job.TopLevelFilesOnly, m_Job.Macros, out var macroDefs);

                    OperationDefinitions = macroDefs;

                    AddLogEntry($"Running batch processing for {m_JobDocuments.Length} file(s)");

                    if (!m_JobDocuments.Any())
                    {
                        throw new UserException("Empty job. No files matching specified filter");
                    }

                }, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                TryShutDownApplication(m_CurrentContext);
            }
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                await m_TaskRunner.Run(() =>
                {
                    var curBatchSize = 0;

                    for (int i = 0; i < m_JobDocuments.Length; i++)
                    {
                        var curAppPrc = m_CurrentContext.CurrentApplicationProcess?.Id;

                        m_CurrentContext.CurrentJobItem = m_JobDocuments[i];
                        var res = TryProcessFile(m_CurrentContext, m_Worker, cancellationToken);

                        TryCloseDocument(m_CurrentContext.CurrentDocument);

                        m_CurrentContext.CurrentDocument = null;

                        ItemProcessed?.Invoke(this, m_CurrentContext.CurrentJobItem);
                        m_State.Progress = (double)(i + 1) / (double)m_JobDocuments.Length;

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
                            AddLogEntry("Closing application as batch size reached the limit");
                            TryShutDownApplication(m_CurrentContext);
                            curBatchSize = 0;
                        }
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
            finally 
            {
                TryShutDownApplication(m_CurrentContext);
            }
        }

        private void OnMacroUserError(IMacroRunnerPopupHandler sender, Exception error)
        {
            var curMacro = m_CurrentContext.CurrentMacro;

            if (curMacro != null)
            {
                curMacro.InternalMacroException = error;
            }
        }

        private void OnRetry(Exception err, int retry, BatchJobContext context)
        {
            AddLogEntry($"Failed to run macro: {err.ParseUserError()}. Retrying (retry #{retry})...");

            ProcessError(err, context);

            m_Logger.Log(err);
        }

        private void OnTimeout(BatchJobContext context)
        {
            AddLogEntry("Operation timed out");
            TryShutDownApplication(context);
        }

        private JobItemDocument[] PrepareJobScope(IXApplication app,
            IEnumerable<string> inputs, string[] filters, bool topLevelOnly, IEnumerable<MacroData> macros, out JobItemOperationMacroDefinition[] macroDefs)
        {
            macroDefs = macros.Select(m => new JobItemOperationMacroDefinition(m)).ToArray();

            var macroDefsLocal = macroDefs;

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
                                AddLogEntry($"Skipping file '{file}'");
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
                .Select(d => new JobItemDocument(d, macroDefsLocal.Select(m => new JobItemMacro(m)).ToArray(), m_AppProvider.Descriptor))
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
                    AddLogEntry($"Closing '{doc.Path}'");

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

                                AddLogEntry("Closing host application");
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

                m_PopupHandler.Stop();
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
            AddLogEntry($"Started processing file {context.CurrentJobItem.Document.Path}");

            context.ForbidSaving = null;
            context.CurrentJobItem.State.Status = BatchJobItemStateStatus_e.InProgress;

            foreach (var macro in context.CurrentJobItem.Operations)
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

                    if (context.CurrentDocument == null)//document was not opened thus reporting error on document item
                    {
                        context.CurrentJobItem.State.ReportError(ex);
                    }
                    else
                    {
                        context.CurrentMacro.State.ReportError(ex);
                    }

                    AddLogEntry(ex.ParseUserError(out _));
                    m_Logger.Log(ex);
                }
            }

            if (context.CurrentJobItem.State.Status != BatchJobItemStateStatus_e.Failed)
            {
                context.CurrentJobItem.State.Status = context.CurrentJobItem.ComposeStatus();
            }

            AddLogEntry($"Processing file '{context.CurrentJobItem.Document.Path}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

            return context.CurrentJobItem.State.Status != BatchJobItemStateStatus_e.Failed;
        }

        private void ProcessError(Exception err, BatchJobContext context)
        {
            if (err is ICriticalException)
            {
                AddLogEntry("Critical error - restarting application");

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

            AddLogEntry($"Running '{context.CurrentMacro.Definition.MacroData.FilePath}' macro");

            try
            {
                context.CurrentMacro.State.Status = BatchJobItemStateStatus_e.InProgress;

                IXDocument macroDoc = null;

                if (!string.IsNullOrEmpty(context.CurrentMacro.Definition.MacroData.Arguments)
                    || context.Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
                {
                    macroDoc = context.CurrentDocument;
                }

                m_MacroRunnerSvc.RunMacro(context.CurrentApplication, context.CurrentMacro.Definition.MacroData.FilePath, null,
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun,
                    context.CurrentMacro.Definition.MacroData.Arguments, macroDoc);

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
                        AddLogEntry("Saving the document");

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

                context.CurrentMacro.State.Status = BatchJobItemStateStatus_e.Succeeded;
            }
            catch (Exception ex)
            {
                context.CurrentMacro.State.Status = BatchJobItemStateStatus_e.Failed;

                if (ex is ICriticalException)
                {
                    throw new CriticalErrorException(ex);
                }
                else if (ex is MacroRunFailedException)
                {
                    throw new UserException($"Failed to run macro '{context.CurrentMacro.Definition.Name}': {ex.Message}", ex);
                }
                else if (ex is INoRetryMacroRunException)
                {
                    context.CurrentMacro.State.ReportError(ex);
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
            AddLogEntry($"Starting host application: {versionInfo.DisplayName}");

            IXApplication app;

            try
            {
                app = m_AppProvider.StartApplication(versionInfo,
                    opts, p => TryAddProcessToJob(p), cancellationToken);

                m_PopupHandler.Start(app);
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
                        AddLogEntry($"Setting the readonly flag to {doc.Path} to prevent upgrade of the file");
                        state |= DocumentState_e.ReadOnly;
                    }
                }
                
                doc.State = state;

                AddLogEntry($"Opening '{doc.Path}'");

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

        private void AddLogEntry(string msg)
        {
            m_LogEntries.Add(msg);
            Log?.Invoke(this, msg);
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                TryShutDownApplication(m_CurrentContext);
                //m_PopupKiller.Dispose();
            }
        }
    }
}
