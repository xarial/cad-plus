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
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;

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

        private readonly IInitiator m_Initiator;

        public HostConsole(IContainerBuilder builder, IInitiator initiator, Type hostApplicationType)
        {
            m_Initiator = initiator;
            m_Initiator.Init(this);

            m_ModulesLoader = new ModulesLoader();
            m_ModulesLoader.Load(this, hostApplicationType);
            
            ConfigureServices?.Invoke(builder);
            Services = builder.Build();

            Application = Services.GetService<IApplication>();

            Initialized?.Invoke();
            Connect?.Invoke();
            Started?.Invoke();
        }

        public void Dispose()
        {
            Disconnect?.Invoke();

            if (Services is IDisposable)
            {
                (Services as IDisposable).Dispose();
            }
        }

        public void ShowPopup<TWindow>(TWindow wnd) where TWindow : Window
            => wnd.ShowDialog();
    }
}
