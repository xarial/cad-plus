//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public interface IBatchRunJobExecutor
    {
        event Action<IJobItem[], DateTime> JobSet;
        event Action<TimeSpan> JobCompleted;
        event Action<IJobItem, bool> ProgressChanged;
        event Action<string> Log;

        Task<bool> ExecuteAsync();
        void Cancel();
    }

    public class BatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;

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
                m_PrgHander.Completed += OnJobCompleted;

                try
                {
                    using (var batchRunner = new BatchRunner(m_AppProvider, null, m_LogWriter, m_PrgHander))
                    {
                        var cancellationToken = m_CurrentCancellationToken.Token;

                        return await batchRunner.BatchRun(m_Job, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch(Exception ex)
                {
                    m_LogWriter.WriteLine(ex.ParseUserError(out _));
                    throw;
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

        private void OnJobCompleted(TimeSpan duration) => JobCompleted?.Invoke(duration);

        private void OnJobScopeSet(IJobItem[] files, DateTime startTime) => JobSet?.Invoke(files, startTime);

        public void Cancel()
        {
            m_CurrentCancellationToken.Cancel();
        }

        private void OnLog(string line)
        {
            Log?.Invoke(line);
        }

        private void OnProgressChanged(IJobItem file, bool result)
        {
            ProgressChanged?.Invoke(file, result);
        }
    }
}
