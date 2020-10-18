//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public interface IBatchRunnerModel 
    {
        AppVersionInfo ParseVersion(string id);
        AppVersionInfo[] InstalledVersions { get; }
        FileFilter[] InputFilesFilter { get; }
        FileFilter[] MacroFilesFilter { get; }
        IBatchRunJobExecutor CreateExecutor(BatchJob job);
    }

    public interface IBatchRunJobExecutor
    {
        event Action<IEnumerable<IJobItemFile>> JobSet;
        event Action<double> ProgressChanged;
        event Action<string> Log;

        Task<bool> ExecuteAsync();
        void Cancel();
    }

    public class BatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<double> ProgressChanged;
        public event Action<IEnumerable<IJobItemFile>> JobSet;
        public event Action<string> Log;

        private CancellationTokenSource m_CurrentCancellationToken;

        private readonly BatchJob m_Job;
        private readonly IApplicationProvider m_AppProvider;

        private readonly LogWriter m_LogWriter;
        private readonly ProgressHandler m_PrgHander;
        
        private bool m_IsExecuting;

        public BatchRunJobExecutor(BatchJob job, IApplicationProvider appProvider) 
        {
            m_Job = job;
            m_AppProvider = appProvider;

            m_LogWriter = new LogWriter();
            m_PrgHander = new ProgressHandler();

            m_IsExecuting = false;
        }

        public async Task<bool> ExecuteAsync()
        {
            if (!m_IsExecuting)
            {
                m_IsExecuting = true;

                m_CurrentCancellationToken = new CancellationTokenSource();

                m_LogWriter.Log += OnLog;
                m_PrgHander.ProgressChanged += OnProgressChanged;
                m_PrgHander.JobScopeSet += OnJobScopeSet;

                try
                {
                    using (var batchRunner = new BatchRunner(m_AppProvider, m_LogWriter, m_PrgHander))
                    {
                        var cancellationToken = m_CurrentCancellationToken.Token;

                        return await batchRunner.BatchRun(m_Job, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    m_LogWriter.Log -= OnLog;
                    m_PrgHander.ProgressChanged -= OnProgressChanged;
                }
            }
            else 
            {
                throw new Exception("Execution is already running");
            }
        }

        private void OnJobScopeSet(IEnumerable<IJobItemFile> files)
        {
            JobSet?.Invoke(files);
        }

        public void Cancel()
        {
            m_CurrentCancellationToken.Cancel();
        }

        private void OnLog(string line)
        {
            Log?.Invoke(line);
        }

        private void OnProgressChanged(double prg)
        {
            ProgressChanged?.Invoke(prg);
        }
    }

    public class BatchRunnerModel : IBatchRunnerModel
    {
        private readonly IApplicationProvider m_AppProvider;

        public BatchRunnerModel(IApplicationProvider appProvider) 
        {
            m_AppProvider = appProvider;
            InstalledVersions = m_AppProvider.GetInstalledVersions().ToArray();

            if (!InstalledVersions.Any()) 
            {
                throw new UserMessageException("Failed to detect any installed version of the host application");
            }
        }

        public FileFilter[] InputFilesFilter => m_AppProvider.InputFilesFilter;

        public FileFilter[] MacroFilesFilter => m_AppProvider.MacroFilesFilter;

        public AppVersionInfo[] InstalledVersions { get; }

        public IBatchRunJobExecutor CreateExecutor(BatchJob job) => new BatchRunJobExecutor(job, m_AppProvider);

        public AppVersionInfo ParseVersion(string id) => m_AppProvider.ParseVersion(id);
    }
}
