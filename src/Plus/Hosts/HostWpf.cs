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
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Hosts
{
    public class HostWpf : IHostWpf
    {
        public event Action Connect;
        public event Action Disconnect;
        public event HostInitializedDelegate Initialized;
        public event Action<IContainerBuilder> ConfigureServices;
        public event HostStartedDelegate Started;

        public Application WpfApplication { get; }

        private IModule[] m_Modules;

        private readonly IModulesLoader m_ModulesLoader;

        private bool m_IsLoaded;

        private readonly IInitiator m_Initiator;
        private readonly IXLogger m_Logger;

        private readonly IServiceContainer m_Services;

        public HostWpf(Application wpfApp, 
            IContainerBuilder builder, IInitiator initiator, Type hostApplicationType)
        {
            m_Initiator = initiator;
            m_Initiator.Init(this);
            
            WpfApplication = wpfApp;
            
            m_IsLoaded = false;
            
            m_ModulesLoader = new ModulesLoader();
            m_Modules = m_ModulesLoader.Load(this, hostApplicationType);
            ConfigureServices?.Invoke(builder);
            m_Services = builder.Build();

            m_Logger = m_Services.GetService<IXLogger>();

            m_Logger.Log("Initiating WPF host", LoggerMessageSeverity_e.Debug);

            Initialized?.Invoke(m_Services.GetService<IApplication>(), m_Services, m_Modules);
            Connect?.Invoke();

            WpfApplication.Activated += OnAppActivated;
            WpfApplication.Exit += OnAppExit;
        }
        
        private void OnAppActivated(object sender, EventArgs e)
        {
            if (!m_IsLoaded)
            {
                m_Logger.Log("AppActivated event called", LoggerMessageSeverity_e.Debug);

                m_IsLoaded = true;
                WpfApplication.Activated -= OnAppActivated;
                
                var parentWnd = WpfApplication.MainWindow != null
                    ? new WindowInteropHelper(WpfApplication.MainWindow).Handle
                    : IntPtr.Zero;

                Started?.Invoke(parentWnd);
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

            if (m_Services is IDisposable) 
            {
                (m_Services as IDisposable).Dispose();
            }

            foreach (var module in m_Modules)
            {
                module.Dispose();
            }
        }

        public void ShowPopup<TWindow>(TWindow wnd) 
            where TWindow : Window
        {
            wnd.Owner = WpfApplication.MainWindow;
            wnd.ShowDialog();
        }
    }
}
