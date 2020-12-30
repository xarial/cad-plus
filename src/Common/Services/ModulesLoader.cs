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
using Xarial.CadPlus.Plus.Attributes;
using System.Reflection;

namespace Xarial.CadPlus.Common.Services
{
    public interface IModulesLoader 
    {
        void Load(IHostApplication host);
    }

    public class ModulesLoader : IModulesLoader
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

            var modules = container.GetExports<IModule, IModuleMetadata>()
                .Where(e => e.Metadata.TargetHostIds.Any() == false
                        || e.Metadata.TargetHostIds.Any(i => Guid.Parse(i).Equals(host.Id)))
                .Select(e => e.Value).ToArray();

            var field = host.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .First(f => f.FieldType == typeof(IModule[]) 
                && f.GetCustomAttributes<ImportManyAttribute>(true).Any());

            field.SetValue(host, modules);

            if (host.Modules?.Any() == true)
            {
                foreach (var module in modules)
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
