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
using Xarial.XCad.Base;

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

        public Application WpfApplication { get; }

        public IServiceProvider Services { get; }

        [ImportMany]
        private IModule[] m_Modules;

        private readonly IModulesLoader m_ModulesLoader;

        private bool m_IsLoaded;

        private readonly IInitiator m_Initiator;
        private readonly IXLogger m_Logger;

        public HostWpf(IApplication app, Application wpfApp, 
            IContainerBuilder builder, IInitiator initiator, IXLogger logger)
        {
            m_Initiator = initiator;
            m_Initiator.Init(this);
            
            Application = app;
            WpfApplication = wpfApp;
            
            m_IsLoaded = false;
            m_Logger = logger;

            m_ModulesLoader = new ModulesLoader();
            m_ModulesLoader.Load(this);
            ConfigureServices?.Invoke(builder);
            Services = builder.Build();
            Initialized?.Invoke();

            WpfApplication.Activated += OnAppActivated;
            WpfApplication.Exit += OnAppExit;
        }
        
        public IntPtr ParentWindow => WpfApplication.MainWindow != null
            ? new WindowInteropHelper(WpfApplication.MainWindow).Handle
            : IntPtr.Zero;

        public virtual IApplication Application { get; }

        private void OnAppActivated(object sender, EventArgs e)
        {
            if (!m_IsLoaded)
            {
                m_Logger.Log("AppActivated event called");

                m_IsLoaded = true;
                WpfApplication.Activated -= OnAppActivated;
                Connect?.Invoke();
                Started?.Invoke();
            }
            else 
            {
                System.Diagnostics.Debug.Assert(false, "Event should be unsubscribed");
            }
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Disconnect?.Invoke();

            if (Services is IDisposable) 
            {
                (Services as IDisposable).Dispose();
            }
        }
    }
}
