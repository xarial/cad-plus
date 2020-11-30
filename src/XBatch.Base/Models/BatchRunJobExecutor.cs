//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.XCad;
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

    public class BatchRunJobExecutor : IBatchRunJobExecutor, IDisposable
    {
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;

        public event Action<string> Log;

        private CancellationTokenSource m_CurrentCancellationToken;

        private readonly BatchJob m_Job;
        
        private readonly LogWriter m_LogWriter;
        private readonly ProgressHandler m_PrgHander;
        
        private bool m_IsExecuting;

        private readonly Func<TextWriter, IProgressHandler, BatchRunner> m_BatchRunnerFact;

        public BatchRunJobExecutor(BatchJob job, Func<TextWriter, IProgressHandler, BatchRunner> batchRunnerFact) 
        {
            m_Job = job;
            
            m_LogWriter = new LogWriter();
            m_PrgHander = new ProgressHandler();

            m_BatchRunnerFact = batchRunnerFact;

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
                    var cancellationToken = m_CurrentCancellationToken.Token;

                    using (var batchRunner = m_BatchRunnerFact.Invoke(m_LogWriter, m_PrgHander)) 
                    {
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
                throw new Exception("Job is currently running");
            }
        }

        private void OnJobCompleted(TimeSpan duration) => JobCompleted?.Invoke(duration);

        private void OnJobScopeSet(IJobItem[] files, DateTime startTime) => JobSet?.Invoke(files, startTime);

        public void Cancel()
        {
            m_CurrentCancellationToken?.Cancel();
        }

        private void OnLog(string line)
        {
            Log?.Invoke(line);
        }

        private void OnProgressChanged(IJobItem file, bool result)
        {
            ProgressChanged?.Invoke(file, result);
        }

        public void Dispose() 
        {
            Cancel();
        }
    }
}
