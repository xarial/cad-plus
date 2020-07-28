using Autofac;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.ExtensionModule;
using Xarial.CadPlus.XToolbar.Properties;
using Xarial.CadPlus.XToolbar.Services;
using Xarial.CadPlus.XToolbar.UI.Forms;
using Xarial.CadPlus.XToolbar.UI.ViewModels;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.XToolbar
{
    public class XToolbarModule : IModule
    {
        [CommandGroupInfo(999)]
        [Title("XToolbar")]
        [Description("Custom toolbar")]
        [Icon(typeof(Resources), nameof(Resources.configure_icon))]
        public enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.configure_icon))]
            [Title("Configure...")]
            [Description("Configure toolbar")]
            [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
            Configuration
        }

        private static Autofac.IContainer m_Container;

        public static TService Resolve<TService>() 
            => m_Container.Resolve<TService>();

        private IXExtension m_Ext;
        private ICommandsManager m_CmdsMgr;
        private ITriggersManager m_TriggersMgr;
        private IMessageService m_Msg;

        public void Load(IXExtension ext)
        {
            m_Ext = ext;
            CreateContainer();
            LoadCommands();
        }

        private void CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(m_Ext);
            builder.RegisterInstance(m_Ext.Application);
            builder.RegisterInstance(m_Ext.Logger);

            builder.RegisterType<MacroEntryPointsExtractor>()
                .As<IMacroEntryPointsExtractor>();

            builder.RegisterType<MacroRunner>()
                .As<IMacroRunner>();

            builder.RegisterType<ToolbarConfigurationProvider>()
                .As<IToolbarConfigurationProvider>();

            builder.RegisterType<SettingsProvider>()
                .As<ISettingsProvider>();

            builder.RegisterType<MessageService>()
                .As<IMessageService>();

            builder.RegisterType<CommandManagerVM>().SingleInstance();

            builder.RegisterType<CommandsManager>()
                .As<ICommandsManager>().SingleInstance();

            builder.RegisterType<TriggersManager>()
                .As<ITriggersManager>().SingleInstance();

            builder.RegisterType<UserSettingsService>();

            m_Container = builder.Build();
        }

        private void LoadCommands() 
        {
            m_Ext.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;

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

                    var popup = m_Ext.CreatePopupWindow<CommandManagerForm>();
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
