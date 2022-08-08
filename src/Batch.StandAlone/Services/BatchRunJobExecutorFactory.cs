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
    public interface IBatchMacroRunJobStandAloneFactory
    {
        IBatchMacroRunJobStandAlone Create(BatchJob job);
    }

    public class BatchMacroRunJobStandAloneFactory : IBatchMacroRunJobStandAloneFactory
    {
        private readonly IJobApplicationProvider m_JobAppProvider;

        private readonly IXLogger m_Logger;
        private readonly IJobProcessManager m_JobPrcMgr;

        private readonly IJobContectResilientWorkerFactory m_WorkerFact;

        private readonly IMacroRunnerPopupHandlerFactory m_PopupHandlerFact;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        private readonly ITaskRunner m_TaskRunner;

        public BatchMacroRunJobStandAloneFactory(IJobApplicationProvider jobAppProvider,
            IBatchApplicationProxy batchAppProxy,
            IJobProcessManager jobPrcMgr, IXLogger logger,
            IJobContectResilientWorkerFactory workerFact, IMacroRunnerPopupHandlerFactory popupHandlerFact, ITaskRunner taskRunner)
        {
            m_JobAppProvider = jobAppProvider;

            m_WorkerFact = workerFact;
            m_BatchAppProxy = batchAppProxy;

            m_PopupHandlerFact = popupHandlerFact;

            m_Logger = logger;

            m_JobPrcMgr = jobPrcMgr;

            m_TaskRunner = taskRunner;
        }

        public IBatchMacroRunJobStandAlone Create(BatchJob job)
        {
            return new BatchMacroRunJobStandAlone(job, m_JobAppProvider.GetApplicationProvider(job), (IBatchApplicationProxy)this.m_BatchAppProxy, m_JobPrcMgr, (IXLogger)this.m_Logger,
                m_WorkerFact.Create(job.Timeout > 0 ? TimeSpan.FromSeconds(job.Timeout) : default),
                m_PopupHandlerFact.Create(job.StartupOptions.HasFlag(StartupOptions_e.Silent)), (ITaskRunner)this.m_TaskRunner);
        }
    }
}
