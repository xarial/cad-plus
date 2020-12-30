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

namespace Xarial.CadPlus.CustomToolbar
{
    [Plus.Attributes.Module]
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

        private IHostExtensionApplication m_Host;
        private ICommandsManager m_CmdsMgr;
        private ITriggersManager m_TriggersMgr;
        private IMessageService m_Msg;

        private List<IIconsProvider> m_IconsProviders;

        public Guid Id => Guid.Parse("A4C69B9C-3DA4-4D1B-B533-A2FF66E13457");

        public CustomToolbarModule() 
        {
            m_IconsProviders = new List<IIconsProvider>();
            
            RegisterIconsProvider(new ImageIconsProvider());
        }

        public void Init(IHostApplication host)
        {
            if (!(host is IHostExtensionApplication))
            {
                throw new InvalidCastException("This module is only availabel for extensions");
            }

            m_Host = (IHostExtensionApplication)host;
            m_Host.Connect += OnConnect;
        }

        private void OnConnect()
        {
            CreateContainer();
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

            builder.RegisterFromServiceProvider<IMacroRunnerExService>(m_Host.Services);
            builder.RegisterFromServiceProvider<IMessageService>(m_Host.Services);
            builder.RegisterFromServiceProvider<IMacroFileFilterProvider>(m_Host.Services);
            builder.RegisterFromServiceProvider<ISettingsProvider>(m_Host.Services);

            builder.RegisterInstance(m_IconsProviders.ToArray());

            m_Container = builder.Build();
        }

        private void LoadCommands()
        {
            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            
            m_CmdsMgr = Resolve<ICommandsManager>();
            m_TriggersMgr = Resolve<ITriggersManager>();
            m_Msg = Resolve<IMessageService>();
        }

        private void OnCommandClick(Commands_e spec)
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

        public void Dispose() => m_Container.Dispose();

        public void RegisterIconsProvider(IIconsProvider provider) => m_IconsProviders.Add(provider);
    }
}
