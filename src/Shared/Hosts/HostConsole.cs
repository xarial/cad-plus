//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Shared.Services;

namespace Xarial.CadPlus.Plus.Shared.Hosts
{
    public class HostConsole : IHostConsole
    {
        public event Action Connect;
        public event Action Disconnect;
        public event Action<IContainerBuilder> ConfigureServices;
        public event HostStartedDelegate Started;
        public event HostInitializedDelegate Initialized;
        
        private readonly IContainerBuilder m_Builder;
        private readonly IModulesLoader m_ModulesLoader;
        private readonly IInitiator m_Initiator;
        private readonly Type m_HostApplicationType;

        private IXLogger m_Logger;
        private IModule[] m_Modules;
        private IServiceProvider m_Services;

        public HostConsole(IContainerBuilder builder, IInitiator initiator, Type hostApplicationType)
        {
            m_Builder = builder;

            m_Initiator = initiator;
            
            m_ModulesLoader = new ModulesLoader();

            m_HostApplicationType = hostApplicationType;
        }

        public void Load() 
        {
            m_Initiator.Init(this);

            m_Modules = m_ModulesLoader.Load(this, m_HostApplicationType);

            ConfigureServices?.Invoke(m_Builder);
            m_Services = m_Builder.Build();

            m_Logger = m_Services.GetService<IXLogger>();

            m_Logger.Log("Initiating Console host", LoggerMessageSeverity_e.Debug);

            Initialized?.Invoke(m_Services.GetService<IApplication>(), m_Services, m_Modules);
            Connect?.Invoke();
            Started?.Invoke(IntPtr.Zero);
        }

        public void Dispose()
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

        public bool? ShowPopup<TWindow>(TWindow wnd) where TWindow : Window
            => wnd.ShowDialog();
    }
}
