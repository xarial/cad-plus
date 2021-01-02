using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Sw;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Batch.Sw
{
    [Module(ApplicationIds.BatchStandAlone)]
    public class BatchSwAppProviderModule : IModule
    {
        public Guid Id => Guid.Parse("4903A43B-0228-471B-A6BD-21AFFF2CCE50");

        private IHost m_Host;
        private IBatchApplication m_App;

        public void Init(IHost host)
        {
            if (!(host.Application is IBatchApplication))
            {
                throw new InvalidCastException("Only batch application is supported for this module");
            }

            m_Host = host;
            m_Host.Connect += OnConnect;
            m_Host.ConfigureServices += OnConfigureServices;

            m_App = (IBatchApplication)host.Application;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.UsingCommonSwServices();
        }

        private void OnConnect()
        {
            var logger = (IXLogger)m_Host.Services.GetService(typeof(IXLogger));
            m_App.RegisterApplicationProvider(new SwApplicationProvider(logger));
        }

        public void Dispose()
        {
        }
    }
}
