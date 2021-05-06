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
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Batch.StandAlone;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Batch.StandAlone.Exceptions;
using Xarial.CadPlus.Common.Utils;
using Xarial.CadPlus.Plus.Shared.Helpers;

namespace Xarial.CadPlus.Batch.Base.Core
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

    public class BatchRunner : IDisposable
    {
        private readonly TextWriter m_UserLogger;
        private readonly IProgressHandler m_ProgressHandler;
        private readonly ICadApplicationInstanceProvider m_AppProvider;
        private readonly IMacroExecutor m_MacroRunnerSvc;

        private readonly IXLogger m_Logger;
        private readonly IJobManager m_JobMgr;

        private readonly Func<TimeSpan?, IResilientWorker<BatchJobContext>> m_WorkerFact;

        private readonly IPopupKiller m_PopupKiller;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        private readonly BatchJob m_Job;

        public BatchRunner(BatchJob job, ICadApplicationInstanceProvider[] appProviders, 
            TextWriter userLogger, IProgressHandler progressHandler,
            IBatchApplicationProxy batchAppProxy,
            IJobManager jobMgr, IXLogger logger,
            Func<TimeSpan?, IResilientWorker<BatchJobContext>> workerFact, IPopupKiller popupKiller)
        {
            m_Job = job;
            m_UserLogger = userLogger;
            m_ProgressHandler = progressHandler;
            m_AppProvider = job.FindApplicationProvider(appProviders);
            m_MacroRunnerSvc = m_AppProvider.MacroRunnerService;
            
            m_WorkerFact = workerFact;
            m_BatchAppProxy = batchAppProxy;

            m_PopupKiller = popupKiller;
            m_PopupKiller.PopupNotClosed += OnPopupNotClosed;

            m_Logger = logger;

            m_JobMgr = jobMgr;
        }

        private void OnPopupNotClosed(Process prc, IntPtr hwnd)
        {
            m_UserLogger.WriteLine("Failed to close the blocking popup window");
        }

        public async Task<bool> BatchRunAsync(CancellationToken cancellationToken = default)
        {
            m_UserLogger.WriteLine($"Batch macro running started");

            var batchStartTime = DateTime.Now;
            
            TimeSpan? timeout = null;

            if (m_Job.Timeout > 0)
            {
                timeout = TimeSpan.FromSeconds(m_Job.Timeout);
            }

            var worker = m_WorkerFact.Invoke(timeout);

            worker.Retry += OnRetry;
            worker.Timeout += OnTimeout;
            
            var context = new BatchJobContext()
            {
                Job = m_Job
            };

            var jobResult = false;

            try
            {
                await Task.Run(() =>
                {
                    m_UserLogger.WriteLine($"Collecting files for processing");

                    var app = EnsureApplication(context, cancellationToken);

                    var allFiles = PrepareJobScope(app, m_Job.Input, m_Job.Filters, m_Job.Macros);

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

                        context.CurrentJobItem = allFiles[i];
                        var res = TryProcessFile(context, worker, cancellationToken);
                        
                        TryCloseDocument(context.CurrentDocument);

                        context.CurrentDocument = null;

                        m_ProgressHandler?.ReportProgress(context.CurrentJobItem, res);

                        if (!res && !m_Job.ContinueOnError)
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

                        if (m_Job.BatchSize > 0 && curBatchSize >= m_Job.BatchSize) 
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
        
        private JobItemDocument[] PrepareJobScope(IXApplication app,
            IEnumerable<string> inputs, string[] filters, IEnumerable<MacroData> macros) 
        {
            var inputFiles = new List<string>();

            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    foreach (var file in Directory.EnumerateFiles(input, "*.*", SearchOption.AllDirectories))
                    {
                        if (FileHelper.MatchesFilter(file, filters))
                        {
                            if (m_AppProvider.CanProcessFile(file))
                            {
                                if (!inputFiles.Contains(input, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    inputFiles.Add(file);
                                }
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

                if (MatchesExtension(f, m_AppProvider.EntityDescriptor.PartFileFilter.Extensions))
                {
                    doc = app.Documents.PreCreate<IXPart>();
                }
                else if (MatchesExtension(f, m_AppProvider.EntityDescriptor.AssemblyFileFilter.Extensions))
                {
                    doc = app.Documents.PreCreate<IXAssembly>();
                }
                else if (MatchesExtension(f, m_AppProvider.EntityDescriptor.DrawingFileFilter.Extensions))
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
            m_UserLogger.WriteLine($"Started processing file {context.CurrentJobItem.FilePath}");

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
                        context.CurrentJobItem.Error = ex;
                    }
                    else 
                    {
                        context.CurrentMacro.Error = ex;
                    }

                    m_UserLogger.WriteLine(ex.ParseUserError(out _));
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

            m_UserLogger.WriteLine($"Processing file '{context.CurrentJobItem.FilePath}' completed. Execution time {DateTime.Now.Subtract(fileProcessStartTime).ToString(@"hh\:mm\:ss")}");

            return context.CurrentJobItem.Status != JobItemStatus_e.Failed;
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

                context.CurrentDocument = EnsureDocument(context.CurrentApplication,
                    context.CurrentJobItem.Document, context.Job.OpenFileOptions, cancellationToken, out bool forbidSaving);

                context.ForbidSaving = forbidSaving;
            }

            if (!context.Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible))
            {
                context.CurrentApplication.Documents.Active = context.CurrentDocument;
            }

            m_UserLogger.WriteLine($"Running '{context.CurrentMacro.FilePath}' macro");

            try
            {
                context.CurrentMacro.Status = JobItemStatus_e.InProgress;
                
                m_MacroRunnerSvc.RunMacro(context.CurrentApplication, context.CurrentMacro.FilePath, null,
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun,
                    context.CurrentMacro.Macro.Arguments, context.CurrentDocument);

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
                else 
                {
                    throw;
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
                    opts, p => TryAddProcessToJob(p), cancellationToken);

                if (opts.HasFlag(StartupOptions_e.Silent)) 
                {
                    m_PopupKiller.Start(app.Process, TimeSpan.FromSeconds(2));
                }
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
                        m_UserLogger.WriteLine($"Setting the readonly flag to {doc.Path} to prevent upgrade of the file");
                        state |= DocumentState_e.ReadOnly;
                    }
                }

                doc.State = state;

                try
                {
                    m_UserLogger.WriteLine($"Opening '{doc.Path}'");

                    doc.Commit(cancellationToken);
                }
                catch (OpenDocumentFailedException ex)
                {
                    throw new UserException($"Failed to open document {doc.Path}: {(ex as OpenDocumentFailedException).Message}", ex);
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
                    if (FileHelper.MatchesFilter(doc.Path, m_AppProvider.EntityDescriptor.PartFileFilter.Extensions)
                        || FileHelper.MatchesFilter(doc.Path, m_AppProvider.EntityDescriptor.AssemblyFileFilter.Extensions)
                        || FileHelper.MatchesFilter(doc.Path, m_AppProvider.EntityDescriptor.DrawingFileFilter.Extensions))
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
            m_AppProvider.Dispose();
        }
    }
}
