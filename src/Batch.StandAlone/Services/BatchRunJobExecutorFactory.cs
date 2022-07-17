//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XCad.Base;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Applications;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IBatchRunJobExecutorFactory
    {
        IBatchRunJobExecutor Create(BatchJob job);
    }

    public class BatchRunJobExecutorFactory : IBatchRunJobExecutorFactory
    {
        private readonly IJobApplicationProvider m_JobAppProvider;

        private readonly IXLogger m_Logger;
        private readonly IJobManager m_JobMgr;

        private readonly IJobContectResilientWorkerFactory m_WorkerFact;

        private readonly IMacroRunnerPopupHandlerFactory m_PopupHandlerFact;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        public BatchRunJobExecutorFactory(IJobApplicationProvider jobAppProvider,
            IBatchApplicationProxy batchAppProxy,
            IJobManager jobMgr, IXLogger logger,
            IJobContectResilientWorkerFactory workerFact, IMacroRunnerPopupHandlerFactory popupHandlerFact)
        {
            m_JobAppProvider = jobAppProvider;

            m_WorkerFact = workerFact;
            m_BatchAppProxy = batchAppProxy;

            m_PopupHandlerFact = popupHandlerFact;

            m_Logger = logger;

            m_JobMgr = jobMgr;
        }

        public IBatchRunJobExecutor Create(BatchJob job)
        {
            return new BatchRunJobExecutor(job, m_JobAppProvider.GetApplicationProvider(job), m_BatchAppProxy, m_JobMgr, m_Logger,
                m_WorkerFact.Create(job.Timeout > 0 ? TimeSpan.FromSeconds(job.Timeout) : default),
                m_PopupHandlerFact.Create(job.StartupOptions.HasFlag(StartupOptions_e.Silent)));
        }
    }
}
