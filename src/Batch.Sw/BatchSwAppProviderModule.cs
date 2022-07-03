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
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Common.Sw.Services;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Batch.Sw
{
    [Module(typeof(IHost), typeof(IBatchApplication))]
    public class BatchSwAppProviderModule : IModule
    {
        private IHost m_Host;
        private IBatchApplication m_App;
        private IServiceProvider m_Services;

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.Connect += OnConnect;
            m_Host.Initialized += OnHostInitialized;
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
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
            var xCadMacroProvider = m_Services.GetService<IXCadMacroProvider>();

            m_App.RegisterApplicationProvider(new SwApplicationProvider(logger, new SwMacroExecutor(xCadMacroProvider, logger), new SwDescriptor()));
        }

        public void Dispose()
        {
        }
    }
}
