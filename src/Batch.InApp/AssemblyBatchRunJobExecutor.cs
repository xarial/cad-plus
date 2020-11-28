using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class MissingJobItemFile : JobItemFile
    {
        public MissingJobItemFile(string fileName, JobItemMacro[] macros) 
            : base($"{fileName}?", macros)
        {
            Status = JobItemStatus_e.Failed;
        }
    }

    public class AssemblyBatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<string> Log;

        private readonly IXApplication m_App;

        private readonly IXComponent[] m_Comps;

        private readonly IMacroRunnerExService m_MacroRunner;
        private readonly IEnumerable<string> m_Macros;
        private readonly bool m_ActivateDocs;

        internal AssemblyBatchRunJobExecutor(IXApplication app, IMacroRunnerExService macroRunnerSvc,
            IXComponent[] components, IEnumerable<string> macros, bool activateDocs) 
        {
            m_App = app;

            m_MacroRunner = macroRunnerSvc;
            m_Comps = components;
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

                        var res = TryProcessFile(jobItem);
                        
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

        private JobItemFile[] PrepareJob() 
        {
            var processedFiles = new List<string>();

            var jobItems = new List<JobItemFile>();

            foreach (var comp in m_Comps) 
            {
                var macros = m_Macros.Select(m => new JobItemMacro(m)).ToArray();

                try
                {
                    var path = comp.Path;

                    if (!processedFiles.Contains(path, StringComparer.CurrentCultureIgnoreCase))
                    {
                        processedFiles.Add(path);
                        jobItems.Add(new JobItemFile(path, macros));
                    }
                }
                catch 
                {
                    jobItems.Add(new MissingJobItemFile(comp.Name, macros));
                }
            }

            return jobItems.ToArray();
        }

        private bool TryProcessFile(JobItemFile file) 
        {
            LogMessage($"Processing '{file.FilePath}'");

            if (file is MissingJobItemFile) 
            {
                LogMessage($"Component '{file.FilePath}' will not be processed as failed to resolve the file path");
                return false;
            }

            IXDocument doc = null;

            try
            {
                doc = m_App.Documents.FirstOrDefault(
                    d => string.Equals(d.Path, file.FilePath,
                    StringComparison.CurrentCultureIgnoreCase));

                if (doc == null)
                {
                    var state = DocumentState_e.Silent;

                    if (!m_ActivateDocs)
                    {
                        state |= DocumentState_e.Hidden;
                    }

                    doc = m_App.Documents.Open(file.FilePath, state);
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
                try
                {
                    if (doc != null)
                    {
                        doc.Close();
                    }
                }
                catch 
                {
                }
            }

            return file.Status == JobItemStatus_e.Succeeded || file.Status == JobItemStatus_e.Warning;
        }

        private void TryRunMacro(JobItemMacro macro, IXDocument doc)
        {
            try
            {
                macro.Status = JobItemStatus_e.InProgress;
                m_MacroRunner.RunMacro(macro.FilePath, null, XCad.Enums.MacroRunOptions_e.UnloadAfterRun, "", doc);
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
