using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class JobItemDocument : JobItemFile
    {
        public IXDocument Document { get; }

        public JobItemDocument(IXDocument doc, JobItemMacro[] macros) : base(doc.Path, macros)
        {
            Document = doc;
        }
    }

    public class AssemblyBatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<string> Log;

        private readonly IXApplication m_App;

        private readonly IXDocument[] m_Docs;

        private readonly IMacroRunnerExService m_MacroRunner;
        private readonly IEnumerable<MacroData> m_Macros;
        private readonly bool m_ActivateDocs;

        internal AssemblyBatchRunJobExecutor(IXApplication app, IMacroRunnerExService macroRunnerSvc,
            IXDocument[] documents, IEnumerable<MacroData> macros, bool activateDocs) 
        {
            m_App = app;

            m_MacroRunner = macroRunnerSvc;
            m_Docs = documents;
            m_Macros = macros;
            m_ActivateDocs = activateDocs;
        }

        public void Cancel()
        {
            //Not supported yet
        }

        public Task<bool> ExecuteAsync()
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
                        prg.Report((double)i / (double)jobItems.Length);
                    }
                }

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
            finally 
            {
                JobCompleted?.Invoke(DateTime.Now - startTime);
            }
        }

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

            var closeDoc = !doc.IsCommitted || doc.State.HasFlag(DocumentState_e.Hidden);

            try
            {
                if (!doc.IsCommitted)
                {
                    var state = DocumentState_e.Silent;

                    if (!m_ActivateDocs)
                    {
                        state |= DocumentState_e.Hidden;
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
                LogMessage($"Failed to process file '{file.FilePath}': {ex.ParseUserError(out _)}");
                file.Status = JobItemStatus_e.Failed;
            }
            finally 
            {
                if (closeDoc)
                {
                    try
                    {
                        if (doc != null && doc.IsCommitted)
                        {
                            doc.Close();
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return file.Status == JobItemStatus_e.Succeeded || file.Status == JobItemStatus_e.Warning;
        }

        private void TryRunMacro(JobItemMacro macro, IXDocument doc)
        {
            try
            {
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
                    errorDesc = "Unknown error";
                }

                LogMessage($"Failed to run macro '{macro.FilePath}': {errorDesc}");

                macro.Status = JobItemStatus_e.Failed;
            }
        }

        private void LogMessage(string msg) 
        {
            Log?.Invoke(msg);
        }
    }
}
