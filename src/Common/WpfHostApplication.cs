//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Module.Init;
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.Common
{
    public class WpfHostApplication : BaseHostApplication, IHostWpfApplication
    {
        public override Guid Id { get; }

        public override IModule[] Modules => throw new NotImplementedException();

        public override event Action Connect;
        public override event Action Disconnect;
        public override event Action Initialized;
        public override event Action<IContainerBuilder> ConfigureServices;
        public override event Action Started;

        private readonly Application m_App;

        public override IServiceProvider Services { get; }

        internal WpfHostApplication(Application app, IServiceProvider svcProvider, Guid hostId)
        {
            m_App = app;
            Id = hostId;
            Services = svcProvider;
            m_App.Activated += OnAppActivated;
            m_App.Exit += OnAppExit;
        }

        public override IntPtr ParentWindow => m_App.MainWindow != null
            ? new WindowInteropHelper(m_App.MainWindow).Handle
            : IntPtr.Zero;

        private void OnAppActivated(object sender, EventArgs e)
        {
            m_App.Activated -= OnAppActivated;
            Started?.Invoke();
            Connect?.Invoke();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            Disconnect?.Invoke();
        }
    }
}
