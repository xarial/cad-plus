using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public class BatchJobHandlersRepository : IDisposable
    {
        private readonly IBatchJobHandlerServiceFactory m_BatchJobHandlerSvcFact;
        private readonly List<IBatchJobHandlerService> m_BatchJobHandlers;
        private readonly IXLogger m_Logger;

        public BatchJobHandlersRepository(IBatchJobHandlerServiceFactory batchJobHandlerSvcFact, IXLogger logger)
        {
            m_BatchJobHandlerSvcFact = batchJobHandlerSvcFact;
            m_BatchJobHandlers = new List<IBatchJobHandlerService>();
            m_Logger = logger;
        }

        public void RunNew(IBatchJob job, string name) 
        {
            var jobHandler = m_BatchJobHandlerSvcFact.Create(job, name, new CancellationTokenSource());

            jobHandler.Disposed += OnDisposed;

            m_BatchJobHandlers.Add(jobHandler);

            jobHandler.Run();
        }

        private void OnDisposed(IBatchJobHandlerService sender)
        {
            m_Logger.Log($"Removing the job handler '{sender.Title}'");

            if (m_BatchJobHandlers.Contains(sender))
            {
                m_BatchJobHandlers.Remove(sender);
            }
        }

        public void Dispose()
        {
            foreach (var jobHandler in m_BatchJobHandlers)
            {
                m_Logger.Log($"Disposing the job handler '{jobHandler.Title}'");
                jobHandler.Dispose();
            }
        }
    }
}
