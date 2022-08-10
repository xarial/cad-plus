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
using Xarial.CadPlus.Plus.Shared.Exceptions;

namespace Xarial.CadPlus.Batch.InApp
{
    public class AssemblyBatchRunJobExecutor : IBatchJob
    {
        public event JobInitializedDelegate Initialized;
        public event JobCompletedDelegate Completed;
        public event JobItemProcessedDelegate ItemProcessed;
        public event JobProgressChangedDelegate ProgressChanged;
        public event JobLogDelegateDelegate Log;

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

        public IReadOnlyList<IJobItem> JobItems { get; private set; }
        public IReadOnlyList<IJobItemOperationDefinition> OperationDefinitions { get; private set; }
        public IReadOnlyList<string> LogEntries => m_LogEntries;

        private readonly ICadDescriptor m_CadDesc;

        private readonly List<string> m_LogEntries;

        internal AssemblyBatchRunJobExecutor(IXApplication app, IMacroExecutor macroRunnerSvc, ICadDescriptor cadDesc,
            IXDocument[] documents, IXLogger logger, IEnumerable<MacroData> macros,
            bool activateDocs, bool allowReadOnly, bool allowRapid, bool autoSaveDocs, IMacroRunnerPopupHandler popupHandler) 
        {
            m_App = app;
            m_Logger = logger;

            m_MacroRunner = macroRunnerSvc;
            m_Docs = documents;
            m_Macros = macros;
            m_CadDesc = cadDesc;

            m_PopupHandler = popupHandler;
            m_PopupHandler.MacroUserError += OnMacroUserError;

            m_ActivateDocs = activateDocs;
            m_AllowReadOnly = allowReadOnly;
            m_AllowRapid = allowRapid;
            m_AutoSaveDocs = autoSaveDocs;

            m_LogEntries = new List<string>();
        }

        private void OnMacroUserError(IMacroRunnerPopupHandler sender, Exception error)
        {
            if (m_CurrentMacro != null) 
            {
                m_CurrentMacro.InternalMacroException = error;
            }
        }

        public void Execute(CancellationToken cancellationToken)
        {
            if (!m_IsExecuted)
            {
                m_IsExecuted = true;

                var startTime = DateTime.Now;

                try
                {
                    LogEntry("Preparing job");

                    m_PopupHandler.Start(m_App);

                    var jobItems = PrepareJob(out var macroDefs);

                    OperationDefinitions = macroDefs;
                    JobItems = jobItems;

                    Initialized?.Invoke(this, jobItems, macroDefs, startTime);

                    for (int i = 0; i < jobItems.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var jobItem = jobItems[i];

                        TryProcessFile(jobItem, cancellationToken);

                        ItemProcessed?.Invoke(this, jobItem);
                        ProgressChanged?.Invoke(this, (double)(i + 1) / (double)jobItems.Length);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw new JobCancelledException();
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                }
                finally
                {
                    m_PopupHandler.Stop();
                    Completed?.Invoke(this, DateTime.Now - startTime);
                }
            }
            else 
            {
                throw new Exception("Job was already executed. This is a transient service and can onlyt be executed once");
            }
        }

        private JobItemDocument[] PrepareJob(out JobItemOperationMacroDefinition[] macroDefs) 
        {
            macroDefs = m_Macros.Select(m => new JobItemOperationMacroDefinition(m)).ToArray();
            var macroDefsLocal = macroDefs;

            var jobItems = new List<JobItemDocument>();

            foreach (var doc in m_Docs) 
            {
                var macros = m_Macros.Select(m => new JobItemMacro(macroDefsLocal.First(d => d.MacroData == m))).ToArray();
                
                jobItems.Add(new JobItemDocument(doc, macros, m_CadDesc));
            }

            return jobItems.ToArray();
        }

        private bool TryProcessFile(JobItemDocument file, CancellationToken cancellationToken) 
        {
            LogEntry($"Processing '{file.Document.Path}'");
            
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
                file.State.ClearIssues();

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

                foreach (var macro in file.Operations)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TryRunMacro(macro, doc);
                }

                if (m_AutoSaveDocs) 
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    doc.Save();
                }

                file.State.Status = file.ComposeStatus();
            }
            catch(Exception ex)
            {
                LogEntry($"Failed to process file '{file.Document.Path}': {ex.ParseUserError()}");
                file.State.Status = JobItemStateStatus_e.Failed;
                file.State.ReportError(ex);
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
                                if (file.State.Status == JobItemStateStatus_e.Succeeded)
                                {
                                    file.State.Status = JobItemStateStatus_e.Warning;
                                }

                                file.State.ReportIssue("The document has been modified and closed without saving changes. Use 'Auto Save' option or call Save API function from the macro directly if it is required to keep the changes", IssueType_e.Warning);
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
                    
                    if (file.State.Status == JobItemStateStatus_e.Succeeded)
                    {
                        file.State.Status = JobItemStateStatus_e.Warning;
                    }

                    file.State.ReportError(ex, "Failed to close document");
                }
            }

            return file.State.Status == JobItemStateStatus_e.Succeeded || file.State.Status == JobItemStateStatus_e.Warning;
        }

        private void TryRunMacro(JobItemMacro macro, IXDocument doc)
        {
            try
            {
                m_CurrentMacro = macro;

                macro.State.ClearIssues();

                macro.State.Status = JobItemStateStatus_e.InProgress;
                
                m_MacroRunner.RunMacro(m_App, macro.Definition.MacroData.FilePath, null, 
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun, macro.Definition.MacroData.Arguments, doc);

                if (macro.InternalMacroException != null)
                {
                    var ex = macro.InternalMacroException;
                    macro.InternalMacroException = null;
                    throw ex;
                }

                macro.State.Status = JobItemStateStatus_e.Succeeded;
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

                LogEntry($"Failed to run macro '{macro.Definition.MacroData.FilePath}': {errorDesc}");

                macro.State.ReportError(ex);
                macro.State.Status = JobItemStateStatus_e.Failed;
            }
        }

        private void LogEntry(string msg)
        {
            m_LogEntries.Add(msg);
            Log?.Invoke(this, msg);
        }

        public void Dispose()
        {
            m_PopupHandler.Dispose();
        }
    }
}
