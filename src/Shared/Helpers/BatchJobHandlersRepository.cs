//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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

        private object m_Lock;

        public BatchJobHandlersRepository(IBatchJobHandlerServiceFactory batchJobHandlerSvcFact, IXLogger logger)
        {
            m_BatchJobHandlerSvcFact = batchJobHandlerSvcFact;
            m_BatchJobHandlers = new List<IBatchJobHandlerService>();
            m_Logger = logger;

            m_Lock = new object();
        }

        public void RunNew(IBatchJob job, string name) 
        {
            var jobHandler = m_BatchJobHandlerSvcFact.Create(job, name, new CancellationTokenSource());

            jobHandler.Disposed += OnDisposed;

            lock (m_Lock)
            {
                m_BatchJobHandlers.Add(jobHandler);
            }

            jobHandler.Run();
        }

        private void OnDisposed(IBatchJobHandlerService sender)
        {
            m_Logger.Log($"Removing the job handler '{sender.Title}'");

            lock (m_Lock)
            {
                if (m_BatchJobHandlers.Contains(sender))
                {
                    m_BatchJobHandlers.Remove(sender);
                }
            }
        }

        public void Dispose()
        {
            lock (m_Lock)
            {
                foreach (var jobHandler in m_BatchJobHandlers)
                {
                    m_Logger.Log($"Disposing the job handler '{jobHandler.Title}'");
                    jobHandler.Dispose();
                }
            }
        }
    }
}
