//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Batch.StandAlone.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Batch.Base.Models
{
    public interface IBatchRunJobExecutorFactory 
    {
        IBatchRunJobExecutor Create(BatchJob job);
    }

    public class BatchRunJobExecutorFactory : IBatchRunJobExecutorFactory
    {
        private readonly IBatchRunnerFactory m_BatchRunnerFactory;
        private readonly IXLogger m_Logger;

        public BatchRunJobExecutorFactory(IBatchRunnerFactory batchRunnerFactory, IXLogger logger) 
        {
            m_Logger = logger;
            m_BatchRunnerFactory = batchRunnerFactory;
        }

        public IBatchRunJobExecutor Create(BatchJob job)
        {
            var logWriter = new JournalWriter(true);
            var prgHandler = new ProgressHandler();

            return new BatchRunJobExecutor(m_BatchRunnerFactory.Create(job, logWriter, prgHandler), logWriter, prgHandler, m_Logger);
        }
    }

    public class BatchRunJobExecutor : IBatchRunJobExecutor, IDisposable
    {
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;

        public event Action<string> Log;

        private readonly JournalWriter m_LogWriter;
        private readonly ProgressHandler m_PrgHander;
        
        private bool m_IsExecuted;

        private readonly BatchRunner m_CurrentBatchRunner;

        private readonly IXLogger m_Logger;

        public BatchRunJobExecutor(BatchRunner batchRunner, JournalWriter logWriter, ProgressHandler prgHandler, IXLogger logger) 
        {
            m_LogWriter = logWriter;
            m_PrgHander = prgHandler;

            m_CurrentBatchRunner = batchRunner;

            m_Logger = logger;

            m_IsExecuted = false;

            m_LogWriter.Log += OnLog;
            m_PrgHander.ProgressChanged += OnProgressChanged;
            m_PrgHander.JobScopeSet += OnJobScopeSet;
            m_PrgHander.Completed += OnJobCompleted;
        }

        public bool Execute(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken).Result;

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!m_IsExecuted)
            {
                m_IsExecuted = true;

                cancellationToken.Register(() => 
                {
                    m_Logger.Log("Trying to cancel batch runner", LoggerMessageSeverity_e.Debug);
                    m_CurrentBatchRunner.TryCancel();
                });

                try
                {
                    return await m_CurrentBatchRunner.BatchRunAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    m_LogWriter.WriteLine(ex.ParseUserError());
                    throw;
                }
                finally
                {
                    m_CurrentBatchRunner.Dispose();
                    m_LogWriter.Log -= OnLog;
                    m_PrgHander.ProgressChanged -= OnProgressChanged;
                }
            }
            else 
            {
                throw new Exception("Job was already executed. This is a transient service and can onlyt be executed once");
            }
        }

        private void OnJobCompleted(TimeSpan duration) => JobCompleted?.Invoke(duration);

        private void OnJobScopeSet(IJobItem[] files, DateTime startTime) => JobSet?.Invoke(files, startTime);

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
            m_CurrentBatchRunner.TryCancel();
        }
    }
}
