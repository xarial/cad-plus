using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using System.ComponentModel.Composition;

namespace Xarial.CadPlus.Common.Services
{
    public class ModulesLoader
    {
        private readonly ISettingsProvider m_SettsProvider;

        public ModulesLoader() 
        {
            m_SettsProvider = new SettingsProvider();
        }

        public void Load(IHostApplication host)
        {
            var modulesDir = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Modules");

            var modulePaths = new List<string>();
            modulePaths.Add(modulesDir);

            var hostSettings = m_SettsProvider.ReadSettings<HostSettings>();

            if (hostSettings.AdditionalModuleFolders != null) 
            {
                modulePaths.AddRange(hostSettings.AdditionalModuleFolders);
            }

            var catalog = CreateDirectoryCatalog(modulePaths.ToArray(), "*.Module.dll");

            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(host);

            if (host.Modules?.Any() == true)
            {
                foreach (var module in host.Modules)
                {
                    module.Init(host);
                }
            }
        }

        private ComposablePartCatalog CreateDirectoryCatalog(string[] paths, string searchPattern)
        {
            var catalog = new AggregateCatalog();

            foreach (var path in paths)
            {
                catalog.Catalogs.Add(new DirectoryCatalog(path, searchPattern));

                foreach (var subDir in Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(subDir, searchPattern));
                }
            }

            return catalog;
        }
    }
}
