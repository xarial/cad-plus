//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.Base.Enums;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.CadPlus.Batch.Base.Services;

namespace Xarial.CadPlus.Batch.InApp
{
    public class AssemblyBatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;
        public event Action<IJobItem, double, bool> ProgressChanged;
        public event Action<string> Log;
        public event Action<string> StatusChanged;

        private readonly IXApplication m_App;

        private readonly IXDocument[] m_Docs;

        private readonly IMacroExecutor m_MacroRunner;
        private readonly IEnumerable<MacroData> m_Macros;
        private readonly bool m_ActivateDocs;
        private readonly bool m_AllowReadOnly;
        private readonly bool m_AllowRapid;
        private readonly bool m_AutoSaveDocs;
        private readonly IXLogger m_Logger;

        private bool m_IsExecuted;

        private JobItemMacro m_CurrentMacro;

        private readonly IMacroRunnerPopupHandler m_PopupHandler;

        internal AssemblyBatchRunJobExecutor(IXApplication app, IMacroExecutor macroRunnerSvc,
            IXDocument[] documents, IXLogger logger, IEnumerable<MacroData> macros,
            bool activateDocs, bool allowReadOnly, bool allowRapid, bool autoSaveDocs, IMacroRunnerPopupHandler popupHandler) 
        {
            m_App = app;
            m_Logger = logger;

            m_MacroRunner = macroRunnerSvc;
            m_Docs = documents;
            m_Macros = macros;

            m_PopupHandler = popupHandler;
            m_PopupHandler.MacroUserError += OnMacroUserError;

            m_ActivateDocs = activateDocs;
            m_AllowReadOnly = allowReadOnly;
            m_AllowRapid = allowRapid;
            m_AutoSaveDocs = autoSaveDocs;
        }

        private void OnMacroUserError(IMacroRunnerPopupHandler sender, Exception error)
        {
            if (m_CurrentMacro != null) 
            {
                m_CurrentMacro.InternalMacroException = error;
            }
        }

        public bool Execute(CancellationToken cancellationToken)
        {
            if (!m_IsExecuted)
            {
                m_IsExecuted = true;

                var startTime = DateTime.Now;

                try
                {
                    LogMessage("Preparing job");

                    m_PopupHandler.Start(m_App);

                    var jobItems = PrepareJob();

                    JobSet?.Invoke(jobItems, startTime);

                    for (int i = 0; i < jobItems.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var jobItem = jobItems[i];

                        StatusChanged?.Invoke($"Processing {jobItem.FilePath}");

                        var res = TryProcessFile(jobItem, cancellationToken);

                        ProgressChanged?.Invoke(jobItem, (double)(i + 1) / (double)jobItems.Length, res);
                    }

                    return true;
                }
                catch (OperationCanceledException)
                {
                    throw new JobCancelledException();
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    return false;
                }
                finally
                {
                    m_PopupHandler.Stop();
                    JobCompleted?.Invoke(DateTime.Now - startTime);
                }
            }
            else 
            {
                throw new Exception("Job was already executed. This is a transient service and can onlyt be executed once");
            }
        }

        public Task<bool> ExecuteAsync(CancellationToken cancellationToken) => Task.FromResult(Execute(cancellationToken));

        private JobItemDocument[] PrepareJob() 
        {
            var jobItems = new List<JobItemDocument>();

            foreach (var doc in m_Docs) 
            {
                var macros = m_Macros.Select(m => new JobItemMacro(m)).ToArray();
                
                jobItems.Add(new JobItemDocument(doc, macros));
            }

            return jobItems.ToArray();
        }

        private bool TryProcessFile(JobItemDocument file, CancellationToken cancellationToken) 
        {
            LogMessage($"Processing '{file.FilePath}'");
            
            var doc = file.Document;

            var closeDoc = false;
            var hideDoc = false;

            if (!doc.IsCommitted)
            {
                closeDoc = true;
            }
            else 
            {
                hideDoc = doc.State.HasFlag(DocumentState_e.Hidden);
            }

            try
            {
                file.ClearIssues();

                if (!doc.IsCommitted)
                {
                    var state = DocumentState_e.Silent;

                    if (!m_ActivateDocs)
                    {
                        state |= DocumentState_e.Hidden;
                    }

                    if (m_AllowReadOnly)
                    {
                        state |= DocumentState_e.ReadOnly;
                    }

                    if (m_AllowRapid)
                    {
                        state |= DocumentState_e.Rapid;
                    }

                    doc.State = state;
                    doc.Commit(cancellationToken);
                }

                if (m_ActivateDocs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    m_App.Documents.Active = doc;
                }

                foreach (var macro in file.Macros)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TryRunMacro(macro, doc);
                }

                if (m_AutoSaveDocs) 
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    doc.Save();
                }

                if (file.Macros.All(m => m.Status == JobItemStatus_e.Succeeded))
                {
                    file.Status = JobItemStatus_e.Succeeded;
                }
                else if (file.Macros.Any(m => m.Status == JobItemStatus_e.Succeeded))
                {
                    file.Status = JobItemStatus_e.Warning;
                }
                else
                {
                    file.Status = JobItemStatus_e.Failed;
                }
            }
            catch(Exception ex)
            {
                LogMessage($"Failed to process file '{file.FilePath}': {ex.ParseUserError()}");
                file.Status = JobItemStatus_e.Failed;
                file.ReportError(ex);
            }
            finally 
            {
                try
                {
                    if (doc != null && doc.IsCommitted)
                    {
                        if (closeDoc)
                        {
                            if (doc.IsDirty) 
                            {
                                if (file.Status == JobItemStatus_e.Succeeded)
                                {
                                    file.Status = JobItemStatus_e.Warning;
                                }

                                file.ReportIssue("The document has been modified and closed without saving changes. Use 'Auto Save' option or call Save API function from the macro directly if it is required to keep the changes");
                            }

                            m_Logger.Log($"Closing '{doc.Path}'", LoggerMessageSeverity_e.Debug);
                            doc.Close();
                        }
                        else if (hideDoc)
                        {
                            m_Logger.Log($"Hiding '{doc.Path}'", LoggerMessageSeverity_e.Debug);
                            var hiddenState = doc.State | DocumentState_e.Hidden;
                            doc.State = hiddenState;
                        }
                    }
                }
                catch(Exception ex)
                {
                    m_Logger.Log(ex);
                    
                    if (file.Status == JobItemStatus_e.Succeeded)
                    {
                        file.Status = JobItemStatus_e.Warning;
                    }

                    file.ReportError(ex, "Failed to close document");
                }
            }

            return file.Status == JobItemStatus_e.Succeeded || file.Status == JobItemStatus_e.Warning;
        }

        private void TryRunMacro(JobItemMacro macro, IXDocument doc)
        {
            try
            {
                m_CurrentMacro = macro;

                macro.ClearIssues();

                macro.Status = JobItemStatus_e.InProgress;
                
                m_MacroRunner.RunMacro(m_App, macro.Macro.FilePath, null, 
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun, macro.Macro.Arguments, doc);

                if (macro.InternalMacroException != null)
                {
                    var ex = macro.InternalMacroException;
                    macro.InternalMacroException = null;
                    throw ex;
                }

                macro.Status = JobItemStatus_e.Succeeded;
            }
            catch(Exception ex)
            {
                string errorDesc;

                if (ex is MacroRunFailedException)
                {
                    errorDesc = (ex as MacroRunFailedException).Message;
                }
                else
                {
                    errorDesc = ex.ParseUserError("Unknown error");
                }

                LogMessage($"Failed to run macro '{macro.FilePath}': {errorDesc}");

                macro.ReportError(ex);
                macro.Status = JobItemStatus_e.Failed;
            }
        }

        private void LogMessage(string msg) => Log?.Invoke(msg);

        public void Dispose()
        {
            m_PopupHandler.Dispose();
        }
    }
}
