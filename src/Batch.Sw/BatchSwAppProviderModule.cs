//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Sw
{
    [Module(typeof(IHostWpf), typeof(IBatchApplication))]
    public class BatchSwAppProviderModule : IModule
    {
        private IHost m_Host;
        private IBatchApplication m_App;
        private IServiceContainer m_Services;

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.Connect += OnConnect;
            m_Host.Initialized += OnHostInitialized;
        }

        private void OnHostInitialized(IApplication app, IServiceContainer svcProvider, IModule[] modules)
        {
            if (!(app is IBatchApplication))
            {
                throw new InvalidCastException("Only batch application is supported for this module");
            }

            m_Services = svcProvider;
            m_App = (IBatchApplication)app;
        }

        private void OnConnect()
        {
            var logger = m_Services.GetService<IXLogger>();
            var macroRunner = m_Services.GetService<IMacroExecutor>(CadApplicationIds.SolidWorks);
            var entDesc = m_Services.GetService<ICadEntityDescriptor>(CadApplicationIds.SolidWorks);

            m_App.RegisterApplicationProvider(new SwApplicationProvider(logger, macroRunner, entDesc));
        }

        public void Dispose()
        {
        }
    }
}
