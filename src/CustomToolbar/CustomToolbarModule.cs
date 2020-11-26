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

namespace Xarial.CadPlus.CustomToolbar
{
    [Export(typeof(IExtensionModule))]
    public class CustomToolbarModule : IExtensionModule
    {
        [Title("Toolbar+")]
        [Description("Toolbar+ configuration")]
        [Icon(typeof(Resources), nameof(Resources.configure_icon))]
        public enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.configure_icon))]
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

            builder.RegisterInstance(m_Host.Extension);
            builder.RegisterInstance(m_Host.Extension.Application);
            builder.RegisterInstance(m_Host.Extension.Logger);

            builder.RegisterType<AppLogger>()
                .As<IXLogger>();

            builder.RegisterType<MacroEntryPointsExtractor>()
                .As<IMacroEntryPointsExtractor>();

            builder.RegisterType<MacroRunner>()
                .As<IMacroRunner>();

            builder.RegisterType<ToolbarConfigurationProvider>()
                .As<IToolbarConfigurationProvider>();

            builder.RegisterType<SettingsProvider>()
                .As<ISettingsProvider>();

            builder.RegisterType<CadAppMessageService>()
                .As<IMessageService>();

            builder.RegisterType<CommandManagerVM>()
                .SingleInstance();

            builder.RegisterType<CommandsManager>()
                .As<ICommandsManager>().SingleInstance();

            builder.RegisterType<TriggersManager>()
                .As<ITriggersManager>().SingleInstance();

            builder.RegisterType<UserSettingsService>();

            m_Container = builder.Build();
        }

        protected virtual void LoadCommands()
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
                            m_CmdsMgr.UpdatedToolbarConfiguration(vm.Settings, vm.ToolbarInfo, vm.IsEditable);
                        }
                        catch (Exception ex)
                        {
                            m_Msg.ShowError(ex, "Failed to save toolbar specification");
                        }
                    }
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}
