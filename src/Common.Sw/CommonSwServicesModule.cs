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
using Xarial.CadPlus.Common.Sw.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.Common.Sw
{
    [Module]
    public class CommonSwServicesModule : IModule
    {
        private IHost m_Host;

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.ConfigureServices += OnConfigureServices;
        }

        private void OnConfigureServices(IContainerBuilder contBuilder)
        {
            contBuilder.Register<SwMacroRunnerExService, IMacroRunnerExService>(CadApplicationIds.SolidWorks);
            contBuilder.Register<SwEntityDescriptor, ICadEntityDescriptor>(CadApplicationIds.SolidWorks);
        }

        public void Dispose()
        {
        }
    }
}
