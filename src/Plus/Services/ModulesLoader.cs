//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Atributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IModulesLoader
    {
        void Load(IHost host);
    }

    public class ModulesLoader : IModulesLoader
    {
        private readonly ISettingsProvider m_SettsProvider;

        public ModulesLoader()
        {
            m_SettsProvider = new SettingsProvider();
        }

        public void Load(IHost host)
        {
            var modulesDir = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Modules");

            var modulePaths = new List<string>();
            modulePaths.Add(modulesDir);

            var hostSettings = m_SettsProvider.ReadSettings<HostSettings>();

            if (hostSettings.AdditionalModuleFolders != null)
            {
                modulePaths.AddRange(hostSettings.AdditionalModuleFolders);
            }

            RemoveDuplicateAndNestedFolders(modulePaths);

            var catalog = CreateDirectoryCatalog(modulePaths.ToArray(), "*.Module.dll");

            var container = new CompositionContainer(catalog);

            bool MatchesHostType(Type hostType) => hostType == null || hostType.IsAssignableFrom(host.GetType());
            bool MatchesAppId(string[] appIds) => appIds?.Any() == false || appIds.Any(i => Guid.Parse(i).Equals(host.Application.Id));

            var modules = container.GetExports<IModule, IModuleMetadata>()
                .Where(e => MatchesHostType(e.Metadata.TargetHostType) && MatchesAppId(e.Metadata.TargetApplicationIds))
                .Select(e => e.Value).ToArray();

            CheckDuplicates(modules);

            var field = host.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .First(f => f.FieldType == typeof(IModule[])
                && f.GetCustomAttributes<ImportManyAttribute>(true).Any());

            if (field == null)
            {
                throw new Exception("Cannot find the modules import field");
            }

            var sorter = new ModulesSorter();
            modules = sorter.Sort(modules);

            field.SetValue(host, modules);

            if (host.Modules?.Any() == true)
            {
                foreach (var module in modules)
                {
                    module.Init(host);
                }
            }
        }

        private void RemoveDuplicateAndNestedFolders(List<string> modulePaths)
        {
            string NormilizePath(string path) => path.TrimEnd('\\') + "\\";

            bool IsInDirectory(string thisDir, string parentDir)
                => NormilizePath(thisDir).StartsWith(NormilizePath(parentDir),
                    StringComparison.CurrentCultureIgnoreCase);

            for (int i = modulePaths.Count - 1; i >= 0; i--)
            {
                var existingDirIndex = -1;

                for (int j = 0; j < modulePaths.Count; j++)
                {
                    if (i != j)
                    {
                        if (IsInDirectory(modulePaths[i], modulePaths[j]))
                        {
                            existingDirIndex = j;
                            break;
                        }
                    }
                }

                if (existingDirIndex != -1)
                {
                    modulePaths.RemoveAt(i);
                }
            }
        }

        private void CheckDuplicates(IModule[] modules)
        {
            var dupModuleIds = modules.GroupBy(m => m.Id).Where(x => x.Count() > 1).Select(m => m.Key).ToArray();

            if (dupModuleIds.Any())
            {
                throw new UserException($"Duplicate modules detected: {string.Join(", ", dupModuleIds)}");
            }
        }

        private ComposablePartCatalog CreateDirectoryCatalog(string[] paths, string searchPattern)
        {
            var catalog = new AggregateCatalog();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(path, searchPattern));

                    foreach (var subDir in Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories))
                    {
                        catalog.Catalogs.Add(new DirectoryCatalog(subDir, searchPattern));
                    }
                }
                else 
                {
                    throw new Exception($"Specified directory '{path}' with modules does not exist");
                }
            }

            return catalog;
        }
    }
}
