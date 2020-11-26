//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.Base.Attributes;
using System.ComponentModel;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.CadPlus.AddIn.Base;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.CadPlus.Common.Services;
using Xarial.XCad.UI.PropertyPage;
using System.Reflection;
using Xarial.XToolkit.Reflection;
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.AddIn.Sw
{
    public class SwCustomHandler : ICustomHandler
    {
        private readonly SwAddInEx m_AddIn;

        internal SwCustomHandler(SwAddInEx addIn) 
        {
            m_AddIn = addIn;
        }

        public IXPropertyPage<TData> CreatePage<TData>()
            => m_AddIn.CreatePage<TData, SwGeneralPropertyManagerPageHandler>();
    }

    [ComVisible(true), Guid("AC45BDF0-66CB-4B08-8127-06C1F0C9452F")]
    [Title("CAD+ Toolset")]
    [Description("The toolset of utilities to complement SOLIDWORKS functionality")]
    public class CadPlusSwAddIn : SwAddInEx
    {
        private class LocalAppConfigBindingRedirectReferenceResolver : AppConfigBindingRedirectReferenceResolver
        {
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
                        typeof(CadPlusSwAddIn).Assembly,
                        typeof(CadPlusCommands_e).Assembly 
                    };
                }
            }   
        }
        
        private readonly IHostExtensionApplication m_Host;

        public CadPlusSwAddIn()
        {
            AppDomain.CurrentDomain.ResolveBindingRedirects(new LocalAppConfigBindingRedirectReferenceResolver());
            m_Host = CreateHost();
        }

        private IHostExtensionApplication CreateHost() => new AddInHostApplication(this, new SwCustomHandler(this));
    }
}
