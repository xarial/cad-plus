//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.XCad;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Batch.Base.Models
{
    public class BatchRunJobExecutor : IBatchRunJobExecutor, IDisposable
    {
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;

        public event Action<string> Log;

        private CancellationTokenSource m_CurrentCancellationToken;

        private readonly BatchJob m_Job;
        
        private readonly JournalWriter m_LogWriter;
        private readonly ProgressHandler m_PrgHander;
        
        private bool m_IsExecuting;

        private readonly Func<BatchJob, TextWriter, IProgressHandler, BatchRunner> m_BatchRunnerFact;

        private BatchRunner m_CurrentBatchRunner;

        public BatchRunJobExecutor(BatchJob job,
            Func<BatchJob, TextWriter, IProgressHandler, BatchRunner> batchRunnerFact) 
        {
            m_Job = job;

            m_LogWriter = new JournalWriter(true);
            m_PrgHander = new ProgressHandler();

            m_BatchRunnerFact = batchRunnerFact;

            m_IsExecuting = false;
        }

        public bool TryExecute() => TryExecuteAsync().Result;

        public async Task<bool> TryExecuteAsync()
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

                    using (m_CurrentBatchRunner = m_BatchRunnerFact.Invoke(m_Job, m_LogWriter, m_PrgHander))
                    {
                        return await m_CurrentBatchRunner.BatchRunAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    m_LogWriter.WriteLine(ex.ParseUserError());
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
            m_CurrentBatchRunner.TryCancel();
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
