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
using Xarial.CadPlus.Common.Sw;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Batch.Sw
{
    [Module(typeof(IHostWpf), typeof(IBatchApplication))]
    public class BatchSwAppProviderModule : IModule
    {
        private IHost m_Host;
        private IBatchApplication m_App;

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.Connect += OnConnect;
        }
        
        private void OnConnect()
        {
            if (!(m_Host.Application is IBatchApplication))
            {
                throw new InvalidCastException("Only batch application is supported for this module");
            }

            m_App = (IBatchApplication)m_Host.Application;

            var logger = m_Host.Services.GetService<IXLogger>();
            m_App.RegisterApplicationProvider(new SwApplicationProvider(logger));
        }

        public void Dispose()
        {
        }
    }
}
