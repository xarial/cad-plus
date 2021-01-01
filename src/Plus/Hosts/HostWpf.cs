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
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Hosts
{
    public class HostWpf : IHostWpf
    {
        public IModule[] Modules => m_Modules;

        public event Action Connect;
        public event Action Disconnect;
        public event Action Initialized;
        public event Action<IContainerBuilder> ConfigureServices;
        public event Action Started;

        private readonly Application m_WpfApp;

        public IServiceProvider Services { get; }

        [ImportMany]
        private IModule[] m_Modules;

        private readonly IModulesLoader m_ModulesLoader;

        public HostWpf(Application wpfApp, IServiceProvider svcProvider)
        {
            m_WpfApp = wpfApp;
            Services = svcProvider;

            m_WpfApp.Activated += OnAppActivated;
            m_WpfApp.Exit += OnAppExit;

            m_ModulesLoader = new ModulesLoader();
            m_ModulesLoader.Load(this);
            Initialized?.Invoke();
            Connect?.Invoke();
        }

        public IntPtr ParentWindow => m_WpfApp.MainWindow != null
            ? new WindowInteropHelper(m_WpfApp.MainWindow).Handle
            : IntPtr.Zero;

        public IApplication Application => throw new NotImplementedException();

        private void OnAppActivated(object sender, EventArgs e)
        {
            m_WpfApp.Activated -= OnAppActivated;
            Started?.Invoke();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            Disconnect?.Invoke();
        }

        public virtual void Dispose()
        {
        }
    }
}
