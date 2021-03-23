//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using System;
using System.ComponentModel;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.UI.Forms;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XToolkit.Services.UserSettings;
using System.ComponentModel.Composition;
using Xarial.XCad.Base;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Plus.Modules;
using System.Collections.Generic;
using Xarial.CadPlus.Common.Attributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.Extensions;

namespace Xarial.CadPlus.CustomToolbar
{
    [Module(typeof(IHostExtension))]
    public class CustomToolbarModule : IToolbarModule
    {
        [Title("Toolbar+")]
        [Description("Toolbar+ configuration")]
        public enum Commands_e
        {
            [IconEx(typeof(Resources), nameof(Resources.configure_vector), nameof(Resources.configure_icon))]
            [Title("Configure...")]
            [Description("Configure custom toolbar")]
            [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
            Configuration
        }

        protected static Autofac.IContainer m_Container;

        public static TService Resolve<TService>()
            => m_Container.Resolve<TService>();

        private IHostExtension m_Host;
        private ICommandsManager m_CmdsMgr;
        private ITriggersManager m_TriggersMgr;
        private IMessageService m_Msg;
        private IXLogger m_Logger;

        private List<IIconsProvider> m_IconsProviders;

        private IServiceProvider m_SvcProvider;

        public CustomToolbarModule() 
        {
            m_IconsProviders = new List<IIconsProvider>();
            
            RegisterIconsProvider(new ImageIconsProvider());
        }

        public void Init(IHost host)
        {
            if (!(host is IHostExtension))
            {
                throw new InvalidCastException("This module is only availabel for extensions");
            }

            m_Host = (IHostExtension)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app, IServiceContainer svcProvider, IModule[] modules)
        {
            m_SvcProvider = svcProvider;
        }

        private void OnConnect()
        {
            CreateContainer();
            
            m_Msg = Resolve<IMessageService>();
            m_Logger = Resolve<IXLogger>();

            LoadCommands();
        }

        protected virtual void CreateContainer()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterInstance(m_Host.Extension).ExternallyOwned();
            builder.RegisterInstance(m_Host.Extension.Application).ExternallyOwned();
            builder.RegisterInstance(m_Host.Extension.Logger);

            builder.RegisterType<AppLogger>()
                .As<IXLogger>();

            builder.RegisterType<MacroEntryPointsExtractor>()
                .As<IMacroEntryPointsExtractor>();

            builder.RegisterType<MacroRunner>()
                .As<IMacroRunner>();

            builder.RegisterType<ToolbarConfigurationProvider>()
                .As<IToolbarConfigurationProvider>();
            
            builder.RegisterType<CommandManagerVM>()
                .SingleInstance();

            builder.RegisterType<CommandsManager>()
                .As<ICommandsManager>().SingleInstance();

            builder.RegisterType<TriggersManager>()
                .As<ITriggersManager>().SingleInstance();

            builder.RegisterType<UserSettingsService>();

            builder.RegisterFromServiceProvider<IMacroExecutor>(m_SvcProvider);
            builder.RegisterFromServiceProvider<IMessageService>(m_SvcProvider);
            builder.RegisterFromServiceProvider<ICadEntityDescriptor>(m_SvcProvider);
            builder.RegisterFromServiceProvider<ISettingsProvider>(m_SvcProvider);

            builder.RegisterInstance(m_IconsProviders.ToArray());

            m_Container = builder.Build();
        }

        private void LoadCommands()
        {
            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            
            m_CmdsMgr = Resolve<ICommandsManager>();
            m_TriggersMgr = Resolve<ITriggersManager>();
        }

        private void OnCommandClick(Commands_e spec)
        {
            try
            {
                switch (spec)
                {
                    case Commands_e.Configuration:

                        var vm = Resolve<CommandManagerVM>();

                        var popup = m_Host.Extension.CreatePopupWindow<CommandManagerForm>();
                        popup.Control.DataContext = vm;
                        popup.Control.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

                        if (popup.ShowDialog() == true)
                        {
                            try
                            {
                                m_CmdsMgr.UpdateToolbarConfiguration(vm.Settings, vm.ToolbarInfo, vm.IsEditable);
                            }
                            catch (Exception ex)
                            {
                                m_Msg.ShowError(ex, "Failed to save toolbar specification");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                m_Msg.ShowError(ex, "Unknown error");
                m_Logger.Log(ex);
            }
        }

        public void Dispose() => m_Container.Dispose();

        public void RegisterIconsProvider(IIconsProvider provider) => m_IconsProviders.Add(provider);
    }
}
