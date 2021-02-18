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
        IModule[] Load(IHost host, Type hostApplicationType);
    }

    public class ModulesLoader : IModulesLoader
    {
        private readonly ISettingsProvider m_SettsProvider;

        public ModulesLoader()
        {
            m_SettsProvider = new SettingsProvider();
        }

        public IModule[] Load(IHost host, Type hostApplicationType)
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
            bool MatchesAppType(Type[] appTypes) => appTypes?.Any() == false || appTypes.Any(t => t.IsAssignableFrom(hostApplicationType));

            var modules = container.GetExports<IModule, IModuleMetadata>()
                .Where(e => MatchesHostType(e.Metadata.TargetHostType) && MatchesAppType(e.Metadata.TargetApplicationTypes))
                .Select(e => e.Value).ToArray();

            CheckDuplicates(modules);

            var sorter = new ModulesSorter();
            modules = sorter.Sort(modules);

            foreach (var module in modules)
            {
                module.Init(host);
            }

            return modules;
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
            var dupModuleTypes = modules.GroupBy(m => m.GetType().FullName).Where(x => x.Count() > 1).Select(m => m.Key).ToArray();

            if (dupModuleTypes.Any())
            {
                throw new UserException($"Duplicate modules detected: {string.Join(", ", dupModuleTypes)}");
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
