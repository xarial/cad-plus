//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Extensions;
using Xarial.CadPlus.Plus;
using Xarial.XCad;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Structures;
using System.Linq;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XToolkit.Reflection;
using System.Reflection;
using System.ComponentModel.Composition;
using System.IO;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.CadPlus.AddIn.Base.Properties;
using Xarial.XCad.Base;
using Xarial.CadPlus.Common.Services;
using Autofac;
using Xarial.CadPlus.Common;
using Autofac.Core.Registration;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Shared.Extensions;
using System.Windows;
using System.Windows.Interop;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.XCad.UI.Structures;
using Xarial.CadPlus.Plus.Attributes;

namespace Xarial.CadPlus.AddIn.Base
{
    public delegate IXPropertyPage<TData> CreatePageDelegate<TData>();

    public class AddInHost : IHostExtension
    {
        public IXExtension Extension { get; }
        
        public event Action Connect;
        public event Action Disconnect;
        public event HostInitializedDelegate Initialized;
        public event Action<IContainerBuilder> ConfigureServices;
        public event HostStartedDelegate Started;
        
        private readonly Dictionary<CommandSpec, Tuple<Delegate, Enum>> m_Handlers;
        
        public IServiceProvider Services => m_SvcProvider;

        private ServiceProvider m_SvcProvider;

        private readonly IModulesLoader m_ModulesLoader;

        private readonly IInitiator m_Initiator;

        private readonly ICadExtensionApplication m_App;

        private readonly IModule[] m_Modules;

        private readonly List<Tuple<EnumCommandGroupSpec, Delegate>> m_RegisteredCommands;

        public AddInHost(ICadExtensionApplication app, IInitiator initiator) 
        {
            m_App = app;
                        
            m_Initiator = initiator;

            try
            {
                m_Initiator.Init(this);

                Extension = m_App.Extension;

                m_RegisteredCommands = new List<Tuple<EnumCommandGroupSpec, Delegate>>();
                m_Handlers = new Dictionary<CommandSpec, Tuple<Delegate, Enum>>();

                Extension.StartupCompleted += OnStartupCompleted;
                Extension.Connect += OnExtensionConnect;
                Extension.Disconnect += OnExtensionDisconnect;
                if (Extension is IXServiceConsumer)
                {
                    (Extension as IXServiceConsumer).ConfigureServices += OnConfigureExtensionServices;
                }

                m_ModulesLoader = new ModulesLoader();
                m_Modules = m_ModulesLoader.Load(this, app.GetType());
            }
            catch (Exception ex)
            {
                new GenericMessageService("CAD+").ShowError(ex, "Failed to init add-in");
                new AppLogger().Log(ex);
                throw;
            }
        }

        private void OnExtensionDisconnect(IXExtension ext) => Dispose();

        private void OnStartupCompleted(IXExtension ext)
        {
            Started?.Invoke(ext.Application.WindowHandle);
        }

        private void OnExtensionConnect(IXExtension ext) 
        {
            try
            {
                var cmdGrp = Extension.CommandManager.AddCommandGroup<CadPlusCommands_e>();
                cmdGrp.CommandClick += OnCommandClick;

                var mainGrpSpec = cmdGrp.Spec;

                Connect?.Invoke();

                var groups = m_RegisteredCommands.GroupBy(x => x.Item1.Id)
                    .Select(g => 
                    {
                        var baseGroupSpecs = g.Where(x => !x.Item1.CmdGrpEnumType.TryGetAttribute<PartialCommandGroupAttribute>(out _))
                            .Select(x => x).ToArray();

                        if (baseGroupSpecs.Any())
                        {
                            if (baseGroupSpecs.Length == 1)
                            {
                                var baseGroupSpec = baseGroupSpecs.First().Item1;

                                return new Tuple<EnumCommandGroupSpec, IEnumerable<Tuple<EnumCommandGroupSpec, Delegate>>>(baseGroupSpec, g);
                            }
                            else
                            {
                                throw new Exception("More than one base group defined");
                            }
                        }
                        else
                        {
                            throw new Exception("No base groups defined");
                        }
                    }).OrderBy(g =>
                    {
                        if (g.Item1.CmdGrpEnumType.TryGetAttribute(out CommandOrderAttribute att))
                        {
                            return att.Order;
                        }
                        else
                        {
                            return 0;
                        }
                    }).ToArray();
                
                foreach (var group in groups) 
                {
                    var baseGroupSpec = group.Item1;

                    //TODO: add support for nested groups
                    baseGroupSpec.Parent = mainGrpSpec;

                    baseGroupSpec.Commands = group.Item2.SelectMany(g =>
                    {
                        var cmds = g.Item1.Commands;
                        foreach (var cmd in cmds)
                        {
                            m_Handlers.Add(cmd, new Tuple<Delegate, Enum>(g.Item2, cmd.Value));
                        }

                        return cmds;
                    }).OrderBy(c =>
                    {
                        int order = -1;
                        if (!c.Value.TryGetAttribute<CommandOrderAttribute>(x => order = x.Order))
                        {
                            order = 0;
                        };
                        return order;
                    }).ToArray();

                    var grp = Extension.CommandManager.AddCommandGroup(baseGroupSpec);

                    grp.CommandClick += OnCommandClick;
                }
            }
            catch (Exception ex)
            {
                new AppLogger().Log(ex);
                new GenericMessageService("CAD+").ShowError("Failed to connect add-in");
                throw;
            }
        }
        
        private void OnConfigureExtensionServices(IXServiceConsumer sender, IXServiceCollection svcColl)
        {
            var builder = new ContainerBuilder();

            builder.Populate(svcColl);

            ConfigureHostServices(builder);

            ConfigureServices?.Invoke(new ContainerBuilderWrapper(builder));

            m_SvcProvider = new ServiceProvider(builder.Build());

            svcColl.Populate(m_SvcProvider.Context);

            Initialized?.Invoke(m_App, m_SvcProvider, m_Modules);
        }

        private void ConfigureHostServices(ContainerBuilder builder) 
        {   
            builder.RegisterInstance(Extension.Application);
            builder.RegisterType<CadAppMessageService>()
                .As<IMessageService>();
            builder.RegisterType<DefaultDocumentAdapter>()
                .As<IDocumentAdapter>();
            builder.RegisterType<SettingsProvider>()
                .As<ISettingsProvider>();
            builder.RegisterType<XCadMacroProvider>()
                .As<IXCadMacroProvider>();
        }

        public void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum
        {
            int? id;

            if (typeof(TCmd).TryGetAttribute(out PartialCommandGroupAttribute att))
            {
                id = att.BaseCommandGroupId;
            }
            else 
            {
                id = null;
            }

            var spec = Extension.CommandManager.CreateSpecFromEnum<TCmd>(null, id);
            
            m_RegisteredCommands.Add(new Tuple<EnumCommandGroupSpec, Delegate>(spec, handler));
        }
        
        private void OnCommandClick(CommandSpec spec)
        {
            if (m_Handlers.TryGetValue(spec, out Tuple<Delegate, Enum> handler))
            {
                handler.Item1.DynamicInvoke(handler.Item2);
            }
            else 
            {
                System.Diagnostics.Debug.Assert(false, "Handler is not registered");
            }
        }
        
        private void OnCommandClick(CadPlusCommands_e spec)
        {
            switch (spec)
            {
                case CadPlusCommands_e.Help:
                    try
                    {
                        System.Diagnostics.Process.Start(Resources.HelpLink);
                    }
                    catch
                    {
                    }
                    break;

                case CadPlusCommands_e.About:
                    ShowPopup(new AboutDialog(
                        new AboutDialogSpec(this.GetType().Assembly,
                        Resources.logo,
                        Licenses.ThirdParty)));
                    break;
            }
        }

        public void Dispose()
        {
            Disconnect?.Invoke();

            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules)
                {
                    module.Dispose();
                }
            }
        }

        public void ShowPopup<TWindow>(TWindow wnd) where TWindow : Window
        {
            var interopHelper = new WindowInteropHelper(wnd);
            interopHelper.Owner = m_App.Extension.Application.WindowHandle;
            wnd.ShowDialog();
        }
    }
}
