//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Extensions;
using Xarial.CadPlus.Module.Init;
using Xarial.CadPlus.Plus;
using Xarial.XCad;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Structures;
using System.Linq;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.CadPlus.AddIn.Base
{
    public delegate IXPropertyPage<TData> CreatePageDelegate<TData>();

    internal class AddInHostApplication : BaseHostApplication, IHostExtensionApplication
    {
        internal const int ROOT_GROUP_ID = 1000;

        public override IntPtr ParentWindow => Extension.Application.WindowHandle;

        public IXExtension Extension { get; }

        public override event ConfigureServicesDelegate ConfigureServices;
        public override event Action Loaded;
        
        public event Action Connect;
        public event Action Disconnect;
        
        private CommandGroupSpec m_ParentGrpSpec;

        private int m_NextId;

        private readonly Dictionary<CommandSpec, Tuple<Delegate, Enum>> m_Handlers;

        private readonly ICustomHandler m_CustomHandlers;

        internal AddInHostApplication(IXExtension ext, ICustomHandler specHandlers) 
        {
            m_CustomHandlers = specHandlers;
            Extension = ext;
            m_NextId = ROOT_GROUP_ID + 1;

            m_Handlers = new Dictionary<CommandSpec, Tuple<Delegate, Enum>>();

            Extension.StartupCompleted += OnStartupCompleted;
        }

        private void OnStartupCompleted(IXExtension ext)
        {
            Loaded?.Invoke();
        }

        internal void InvokeConnect() 
        {
            m_ParentGrpSpec = Extension.CommandManager.CommandGroups.First(g => g.Spec.Id == ROOT_GROUP_ID).Spec;

            Connect?.Invoke();
        }

        internal void InvokeDisconnect()
        {
            Disconnect?.Invoke();
        }

        internal void InvokeConfigureServices(IXServiceCollection svcColl)
        {
            ConfigureServices?.Invoke(svcColl);
        }

        public void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum
        {
            var spec = Extension.CommandManager.CreateSpecFromEnum<TCmd>(m_NextId++, m_ParentGrpSpec);
            spec.Parent = m_ParentGrpSpec;
            
            var grp = Extension.CommandManager.AddCommandGroup(spec);

            foreach (var cmd in grp.Spec.Commands) 
            {
                m_Handlers.Add(cmd, new Tuple<Delegate, Enum>(handler, (Enum)Enum.ToObject(typeof(TCmd), cmd.UserId)));
            }

            grp.CommandClick += OnCommandClick;
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

        public IXPropertyPage<TData> CreatePage<TData>() 
        {
            if (m_CustomHandlers != null)
            {
                return m_CustomHandlers.CreatePage<TData>();
            }
            else
            {
                return Extension.CreatePage<TData>();
            }
        }
    }
}
