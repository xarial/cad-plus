//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xarial.CadPlus.AddIn.Base.Properties;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Reflection;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using Xarial.CadPlus.Plus;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.AddIn.Base
{
    public class AddInController : IDisposable
    {
        private class LocalAppConfigBindingRedirectReferenceResolver : AppConfigBindingRedirectReferenceResolver 
        {
            protected override Assembly[] GetRequestingAssemblies(Assembly requestingAssembly)
                => new Assembly[] { requestingAssembly ?? typeof(AddInController).Assembly };
        }

        static AddInController() 
        {
            AppDomain.CurrentDomain.ResolveBindingRedirects(new LocalAppConfigBindingRedirectReferenceResolver());
        }

        private readonly AddInHostApplication m_AddInApp;

        [ImportMany]
        private IEnumerable<IExtensionModule> m_Modules;

        public AddInController(IXExtension ext) 
        {
            m_AddInApp = new AddInHostApplication(ext);

            var modulesDir = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Modules");

            var catalog = CreateDirectoryCatalog(modulesDir, "*.Module.dll");

            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);
            
            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules)
                {
                    module.Init(m_AddInApp);
                }
            }
        }

        public void Connect() 
        {
            var cmdGrp = m_AddInApp.Extension.CommandManager.AddCommandGroup<CadPlusCommands_e>();
            cmdGrp.CommandClick += OnCommandClick;

            m_AddInApp.InvokeConnect();
        }

        public void ConfigureServices(IXServiceCollection svcColl) 
        {
            svcColl.AddOrReplace<IXLogger, AppLogger>();
            m_AddInApp.InvokeConfigureServices(svcColl);
        }

        public void Disconnect() 
        {
            m_AddInApp.InvokeDisconnect();
            Dispose();
        }
                
        private ComposablePartCatalog CreateDirectoryCatalog(string path, string searchPattern)
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(path, searchPattern));

            foreach (var subDir in Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories)) 
            {
                catalog.Catalogs.Add(new DirectoryCatalog(subDir, searchPattern));
            }

            return catalog;
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
                    AboutDialog.Show(this.GetType().Assembly, Resources.logo,
                        m_AddInApp.Extension.Application.WindowHandle);
                    break;
            }
        }
        
        public void Dispose()
        {
            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules)
                {
                    module.Dispose();
                }
            }
        }
    }
}
