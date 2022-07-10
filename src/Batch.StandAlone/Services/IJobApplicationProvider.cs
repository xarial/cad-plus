using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IJobApplicationProvider
    {
        ICadApplicationInstanceProvider GetApplicationProvider(BatchJob job);
    }

    public class JobApplicationProvider : IJobApplicationProvider 
    {
        private readonly IBatchApplication m_BatchApp;

        public JobApplicationProvider(IBatchApplication batchApp) 
        {
            m_BatchApp = batchApp;
        }

        public ICadApplicationInstanceProvider GetApplicationProvider(BatchJob job)
        {
            var appProvider = m_BatchApp.ApplicationProviders.FirstOrDefault(
                p => string.Equals(p.Descriptor.ApplicationId, job.ApplicationId,
                StringComparison.CurrentCultureIgnoreCase));

            if (appProvider == null)
            {
                throw new UserException("Failed to find the application provider for this job file");
            }

            return appProvider;
        }
    }
}
