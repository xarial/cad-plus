using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IBatchRunnerFactory
    {
        BatchRunner Create(BatchJob job, TextWriter journalWriter, IProgressHandler progressHandler);
    }

    public class BatchRunnerFactory : IBatchRunnerFactory
    {
        private readonly ICadApplicationInstanceProvider[] m_AppProviders;

        private readonly IXLogger m_Logger;
        private readonly IJobManager m_JobMgr;

        private readonly IJobContectResilientWorkerFactory m_WorkerFact;

        private readonly IPopupKillerFactory m_PopupKillerFact;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        public BatchRunnerFactory(ICadApplicationInstanceProvider[] appProviders,
            IBatchApplicationProxy batchAppProxy,
            IJobManager jobMgr, IXLogger logger,
            IJobContectResilientWorkerFactory workerFact, IPopupKillerFactory popupKillerFactory)
        {
            m_AppProviders = appProviders;

            m_WorkerFact = workerFact;
            m_BatchAppProxy = batchAppProxy;

            m_PopupKillerFact = popupKillerFactory;
            
            m_Logger = logger;

            m_JobMgr = jobMgr;
        }

        public BatchRunner Create(BatchJob job, TextWriter journalWriter, IProgressHandler progressHandler)
            => new BatchRunner(job, journalWriter, progressHandler,
                job.FindApplicationProvider(m_AppProviders),
                m_BatchAppProxy, m_JobMgr, m_Logger,
                m_WorkerFact.Create(job.Timeout > 0 ? TimeSpan.FromSeconds(job.Timeout) : default),
                m_PopupKillerFact.Create());
    }
}
