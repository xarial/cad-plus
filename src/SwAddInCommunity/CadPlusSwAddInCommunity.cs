﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xarial.CadPlus.AddIn.Sw.Services;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.DI;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XToolkit.Helpers;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.AddIn.Sw
{
    [ComVisible(true), Guid("88C2C3F5-397B-41CE-8438-B31C7B5A231F")]
    [Title("CAD+ Toolset (Community)")]
    [Description("The toolset of utilities to complement SOLIDWORKS functionality")]
    public class CadPlusSwAddInCommunity : SwAddInEx
    {
        private class LocalAppConfigBindingRedirectReferenceResolver : AppConfigBindingRedirectReferenceResolver
        {
            internal LocalAppConfigBindingRedirectReferenceResolver() : base("CAD+ SOLIDWORKS Add-In (Community)")
            {
            }

            protected override Assembly[] GetRequestingAssemblies(Assembly requestingAssembly)
            {
                if (requestingAssembly != null)
                {
                    return new Assembly[] { requestingAssembly };
                }
                else
                {
                    return new Assembly[]
                    {
                        typeof(CadPlusSwAddInCommunity).Assembly,
                        typeof(CadPlusCommands_e).Assembly
                    };
                }
            }
        }

        private static readonly AssemblyResolver m_AssmResolver;

        static CadPlusSwAddInCommunity()
        {
            m_AssmResolver = new AssemblyResolver(AppDomain.CurrentDomain, "CAD+ Toolset (Community)");
            m_AssmResolver.RegisterAssemblyReferenceResolver(new LocalAppConfigBindingRedirectReferenceResolver());
        }

        private readonly AddInHost m_Host;

        public CadPlusSwAddInCommunity()
        {
            m_Host = new AddInHost(new SwAddinCommunityApplication(this), new Initiator());
            m_Host.ConfigureServices += OnConfigureModuleServices;
        }

        protected override IXServiceCollection CreateServiceCollection()
            => new SimpleInjectorServiceCollectionContainerBuilder();

        private void OnConfigureModuleServices(IContainerBuilder builder)
        {
            builder.RegisterSingleton<IPropertyPageHandlerProvider, CadPlusComunityPropertyPageHandlerProvider>();

            builder.RegisterAdapter<IXApplication, ISwApplication>(LifetimeScope_e.Singleton);

            builder.RegisterAdapter<ICadSpecificServiceFactory<IMacroExecutor>, IMacroExecutor>(f => f.GetService(CadApplicationIds.SolidWorks), LifetimeScope_e.Singleton);
            builder.RegisterAdapter<ICadSpecificServiceFactory<ICadDescriptor>, ICadDescriptor>(f => f.GetService(CadApplicationIds.SolidWorks), LifetimeScope_e.Singleton);
        }
    }
}
