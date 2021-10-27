//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using System;
using System.ComponentModel;
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
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using System.IO;

namespace Xarial.CadPlus.CustomToolbar
{
    [Title("Toolbar+")]
    [Description("Toolbar+ configuration")]
    [CommandGroupInfo((int)CadCommandGroupIds_e.Toolbar)]
    [CommandOrder(5)]
    public enum Commands_e
    {
        [IconEx(typeof(Resources), nameof(Resources.configure_vector), nameof(Resources.configure_icon))]
        [Title("Configure Custom Toolbars...")]
        [Description("Configure custom toolbar")]
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        Configuration
    }

    [Module(typeof(IHostExtension))]
    public class ToolbarModule : IToolbarModule
    {
        public event MacroRunningDelegate MacroRunning;

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
        private IToolbarModuleProxy m_ToolbarProxy;
        private IToolbarConfigurationManager m_ToolbarConfMgr;

        public ToolbarModule() 
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

            m_ToolbarProxy = Resolve<IToolbarModuleProxy>();
            m_ToolbarProxy.RequestMacroRunning += OnRequestMacroRunning;

            m_ToolbarConfMgr = Resolve<IToolbarConfigurationManager>();

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);

            try
            {
                m_ToolbarConfMgr.Load();
                var workDir = Path.GetDirectoryName(m_ToolbarConfMgr.FilePath);
                LoadCommands(m_ToolbarConfMgr.Toolbar, workDir);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_Msg.ShowError(ex, "Failed to load Toolbar+ commands");
            }
        }

        private void OnRequestMacroRunning(EventType_e eventType, MacroRunningArguments args)
            => MacroRunning?.Invoke(eventType, args);

        protected virtual void CreateContainer()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterInstance(m_Host.Extension).ExternallyOwned();
            builder.RegisterInstance(m_Host.Extension.Application).ExternallyOwned();
            builder.RegisterInstance(m_Host.Extension.Logger);
            
            builder.RegisterType<MacroEntryPointsExtractor>()
                .As<IMacroEntryPointsExtractor>();

            builder.RegisterType<FilePathResolver>()
                .As<IFilePathResolver>();
            
            builder.RegisterType<MacroRunner>()
                .As<IMacroRunner>().SingleInstance();

            builder.RegisterType<ToolbarModuleProxy>()
                .As<IToolbarModuleProxy>().SingleInstance();

            builder.RegisterType<ToolbarConfigurationProvider>()
                .As<IToolbarConfigurationProvider>();

            builder.RegisterType<ToolbarConfigurationManager>()
                .As<IToolbarConfigurationManager>().SingleInstance();

            builder.RegisterType<CommandManagerVM>();

            builder.RegisterType<CommandGroupVM>();
            builder.RegisterType<CommandMacroVM>();
            
            builder.RegisterType<CommandsManager>()
                .As<ICommandsManager>().SingleInstance();

            builder.RegisterType<TriggersManager>()
                .As<ITriggersManager>().SingleInstance();

            builder.RegisterType<UserSettingsService>();

            builder.RegisterFromServiceProvider<IMacroExecutor>(m_SvcProvider);
            builder.RegisterFromServiceProvider<IMessageService>(m_SvcProvider);
            builder.RegisterFromServiceProvider<ICadDescriptor>(m_SvcProvider);
            builder.RegisterFromServiceProvider<ISettingsProvider>(m_SvcProvider);

            builder.RegisterInstance(m_IconsProviders.ToArray());

            m_Container = builder.Build();
        }

        private void LoadCommands(CustomToolbarInfo toolbarInfo, string workDir)
        {   
            m_CmdsMgr = Resolve<ICommandsManager>();
            m_TriggersMgr = Resolve<ITriggersManager>();

            m_CmdsMgr.CreateCommandGroups(toolbarInfo, workDir);
            m_TriggersMgr.Load(toolbarInfo, workDir);
        }

        private void OnCommandClick(Commands_e spec)
        {
            try
            {
                switch (spec)
                {
                    case Commands_e.Configuration:

                        var vm = Resolve<CommandManagerVM>();

                        vm.Load(m_ToolbarConfMgr.Toolbar.Clone(), m_ToolbarConfMgr.FilePath);
                        
                        var popup = m_Host.Extension.CreatePopupWindow<CommandManagerForm>();
                        popup.Control.DataContext = vm;
                        popup.Control.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

                        if (popup.ShowDialog() == true)
                        {
                            try
                            {
                                m_ToolbarConfMgr.Toolbar = vm.ToolbarInfo;
                                m_ToolbarConfMgr.FilePath = vm.ToolbarSpecificationPath;

                                if (m_ToolbarConfMgr.SettingsChanged)
                                {
                                    m_ToolbarConfMgr.SaveSettings();
                                }

                                if (m_ToolbarConfMgr.ToolbarChanged) 
                                {
                                    m_ToolbarConfMgr.SaveToolbar();
                                    
                                    //TODO: make this message SOLIDWORKS specific only as other CAD systems might have different conditions for loading of toolbar
                                    m_Msg.ShowInformation("Toolbar settings have been changed. Restart SOLIDWORKS to load the command manager and menu. If commands in the toolbar have been changed (added or removed) then it might be required to restart SOLIDWORKS twice for the changes to be applied");
                                }
                            }
                            catch (Exception ex)
                            {
                                m_Logger.Log(ex);
                                m_Msg.ShowError(ex, "Failed to save toolbar specification");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_Msg.ShowError(ex, "Unknown error");
                m_Logger.Log(ex);
            }
        }

        public void Dispose()
        {
            m_ToolbarProxy.RequestMacroRunning -= OnRequestMacroRunning;
            m_Container.Dispose();
        }

        public void RegisterIconsProvider(IIconsProvider provider) => m_IconsProviders.Add(provider);
    }
    
    public interface IToolbarModuleProxy 
    {
        event MacroRunningDelegate RequestMacroRunning;
        void CallMacroRunning(EventType_e trigger, MacroRunningArguments args);
    }

    internal class ToolbarModuleProxy : IToolbarModuleProxy
    {
        public event MacroRunningDelegate RequestMacroRunning;

        public void CallMacroRunning(EventType_e eventType, MacroRunningArguments args)
            => RequestMacroRunning?.Invoke(eventType, args);
    }
}
