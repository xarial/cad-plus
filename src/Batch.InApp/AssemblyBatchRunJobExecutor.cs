//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Models;
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

namespace Xarial.CadPlus.Batch.InApp
{
    public class AssemblyBatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<string> Log;

        private readonly IXApplication m_App;

        private readonly IXDocument[] m_Docs;

        private readonly IMacroExecutor m_MacroRunner;
        private readonly IEnumerable<MacroData> m_Macros;
        private readonly bool m_ActivateDocs;
        private readonly bool m_AllowReadOnly;
        private readonly bool m_AllowRapid;
        private readonly bool m_AutoSaveDocs;
        private readonly IXLogger m_Logger;

        internal AssemblyBatchRunJobExecutor(IXApplication app, IMacroExecutor macroRunnerSvc,
            IXDocument[] documents, IXLogger logger, IEnumerable<MacroData> macros,
            bool activateDocs, bool allowReadOnly, bool allowRapid, bool autoSaveDocs) 
        {
            m_App = app;
            m_Logger = logger;

            m_MacroRunner = macroRunnerSvc;
            m_Docs = documents;
            m_Macros = macros;

            m_ActivateDocs = activateDocs;
            m_AllowReadOnly = allowReadOnly;
            m_AllowRapid = allowRapid;
            m_AutoSaveDocs = autoSaveDocs;
        }

        public void Cancel()
        {
            //Not supported yet
        }
        
        public bool TryExecute()
        {
            var startTime = DateTime.Now;

            try
            {
                using (var prg = m_App.CreateProgress())
                {
                    LogMessage("Preparing job");

                    var jobItems = PrepareJob();

                    JobSet?.Invoke(jobItems, startTime);

                    for (int i = 0; i < jobItems.Length; i++)
                    {
                        var jobItem = jobItems[i];

                        prg.SetStatus($"Processing {jobItem.FilePath}");

                        var res = TryProcessFile(jobItem, default);

                        ProgressChanged?.Invoke(jobItem, res);
                        prg.Report((double)(i + 1) / (double)jobItems.Length);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                return false;
            }
            finally
            {
                JobCompleted?.Invoke(DateTime.Now - startTime);
            }
        }

        public Task<bool> TryExecuteAsync() => Task.FromResult(TryExecute());

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
                    m_App.Documents.Active = doc;
                }

                foreach (var macro in file.Macros)
                {
                    TryRunMacro(macro, doc);
                }

                if (m_AutoSaveDocs) 
                {
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
                macro.ClearIssues();

                macro.Status = JobItemStatus_e.InProgress;
                
                m_MacroRunner.RunMacro(m_App, macro.Macro.FilePath, null, 
                    XCad.Enums.MacroRunOptions_e.UnloadAfterRun, macro.Macro.Arguments, doc);

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

        private void LogMessage(string msg) 
        {
            Log?.Invoke(msg);
        }
    }
}
