//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Hosts
{
    public class HostConsole : IHostConsole
    {
        public IntPtr ParentWindow => IntPtr.Zero;

        public event Action Connect;
        public event Action Disconnect;
        public event Action Initialized;
        public event Action<IContainerBuilder> ConfigureServices;
        public event Action Started;

        public IModule[] Modules => m_Modules;

        public IServiceProvider Services { get; }

        public IApplication Application { get; }

        [ImportMany]
        private IModule[] m_Modules;

        private readonly IModulesLoader m_ModulesLoader;

        public HostConsole(IApplication app, IServiceProvider svcProvider)
        {
            Application = app;
            Services = svcProvider;

            m_ModulesLoader = new ModulesLoader();
            m_ModulesLoader.Load(this);
            Initialized?.Invoke();
            Connect?.Invoke();

            Started?.Invoke();
        }

        public void Dispose()
        {
        }
    }
}
