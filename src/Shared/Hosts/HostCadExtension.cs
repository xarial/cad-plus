//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Properties;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands.Structures;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Plus.Shared.Hosts
{
    public delegate IXPropertyPage<TData> CreatePageDelegate<TData>();

    [Title("CAD+")]
    [Description("CAD+ Toolset features and options")]
    [CommandGroupInfo((int)CadCommandGroupIds_e.Main)]
    public enum CadPlusCommands_e
    {
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [IconEx(typeof(Resources), nameof(Resources.help_vector), nameof(Resources.help_icon))]
        [Title("Help...")]
        Help,

        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [IconEx(typeof(Resources), nameof(Resources.about_vector), nameof(Resources.about_icon))]
        [Title("About...")]
        About
    }

    public class HostCadExtension : IHostCadExtension
    {
        private class DefaultDocumentAdapter : IDocumentAdapter
        {
            public void ApplyChanges(IXDocument doc)
            {
            }

            public void DisposeDocument(IXDocument doc)
            {
            }

            public IXDocument GetDocumentReplacement(IXDocument doc, bool allowReadOnly)
                => doc;
        }

        public IXExtension Extension { get; }

        public event Action Connect;
        public event Action Disconnect;
        public event HostInitializedDelegate Initialized;
        public event Action<IContainerBuilder> ConfigureServices;
        public event HostStartedDelegate Started;

        private readonly Dictionary<CommandSpec, Tuple<Delegate, Enum>> m_Handlers;

        public IServiceProvider Services => m_SvcProvider;

        private readonly IModulesLoader m_ModulesLoader;
        private readonly IInitiator m_Initiator;
        private readonly ICadExtensionApplication m_App;
        private readonly List<Tuple<EnumCommandGroupSpec, Delegate>> m_RegisteredCommands;

        private IModule[] m_Modules;
        private IServiceProvider m_SvcProvider;

        public HostCadExtension(ICadExtensionApplication app, IInitiator initiator)
        {
            m_App = app;

            m_Initiator = initiator;

            Extension = m_App.Extension;
            m_ModulesLoader = new ModulesLoader();

            m_RegisteredCommands = new List<Tuple<EnumCommandGroupSpec, Delegate>>();
            m_Handlers = new Dictionary<CommandSpec, Tuple<Delegate, Enum>>();
        }

        public void Load() 
        {
            try
            {
                m_Initiator.Init(this);

                Extension.StartupCompleted += OnStartupCompleted;
                Extension.Connect += OnExtensionConnect;
                Extension.Disconnect += OnExtensionDisconnect;
                if (Extension is IXServiceConsumer)
                {
                    (Extension as IXServiceConsumer).ConfigureServices += OnConfigureExtensionServices;
                }

                m_Modules = m_ModulesLoader.Load(this, m_App.GetType());
            }
            catch (Exception ex)
            {
                new GenericMessageService().ShowError(ex, "Failed to init add-in");
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
                new GenericMessageService().ShowError("Failed to connect add-in");
                throw;
            }
        }

        private void OnConfigureExtensionServices(IXServiceConsumer sender, IXServiceCollection svcColl)
        {
            if (!(svcColl is IContainerBuilder))
            {
                throw new InvalidCastException($"{nameof(IXServiceCollection)} is not replaced with {nameof(IContainerBuilder)}");
            }

            var builder = (IContainerBuilder)svcColl;

            ConfigureHostServices(builder);

            ConfigureServices?.Invoke(builder);

            m_SvcProvider = builder.Build();

            Initialized?.Invoke(m_App, m_SvcProvider, m_Modules);
        }

        private void ConfigureHostServices(IContainerBuilder builder)
        {
            builder.RegisterInstance(Extension);
            builder.RegisterInstance(Extension.Application);
            builder.RegisterSingleton<IMessageService, CadAppMessageService>().UsingParameters(Parameter<Type[]>.Any(UserException.AdditionalUserExceptions));
            builder.RegisterSingleton<IParentWindowProvider, CadParentWindowProvider>();
            builder.RegisterSingleton<IDocumentAdapter, DefaultDocumentAdapter>();
            builder.RegisterSingleton<IXCadMacroProvider, XCadMacroProvider>();
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
                    try
                    {
                        m_SvcProvider.GetService<IAboutService>().ShowAbout(m_App.GetType().Assembly,
                            Resources.logo);
                    }
                    catch
                    {
                    }
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

        public bool? ShowPopup<TWindow>(TWindow wnd) where TWindow : Window
            => m_App.Extension.CreatePopupWindow(wnd).ShowDialog();
    }
}
