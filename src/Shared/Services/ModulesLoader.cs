//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class ModulesLoader : IModulesLoader
    {
        private const string LOCAL_MODULES_FOLDER_NAME = "Modules";
        private const string USER_MODULES_FOLDER_NAME = "Plus";
        private const string MODULES_FILTER = "*.Module.dll";

        private readonly ISettingsProvider m_SettsProvider;
        private readonly IXLogger m_Logger;

        public ModulesLoader()
        {
            m_SettsProvider = new SettingsProvider();
            m_Logger = new AppLogger();
        }

        public IModule[] Load(IHost host, Type hostApplicationType)
        {
            var modulesDir = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), LOCAL_MODULES_FOLDER_NAME);

            var modulePaths = new List<string>();

            modulePaths.Add(modulesDir);

            var plusDir = Path.Combine(Locations.AppDirectoryPath, USER_MODULES_FOLDER_NAME);

            if (Directory.Exists(plusDir))
            {
                modulePaths.Add(plusDir);
            }

            var hostSettings = m_SettsProvider.ReadSettings<HostSettings>();

            if (hostSettings.AdditionalModuleFolders != null)
            {
                modulePaths.AddRange(hostSettings.AdditionalModuleFolders);
            }

            var catalog = CreateDirectoryCatalog(FileSystemUtils.GetTopFolders(modulePaths), MODULES_FILTER);

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
                m_Logger.Log($"Initiating '{module.GetType().FullName}' module");

                module.Init(host);
            }

            return modules;
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
                    m_Logger.Log($"Adding modules directory: '{path}' to catalog");

                    catalog.Catalogs.Add(new DirectoryCatalog(path, searchPattern));

                    foreach (var subDir in Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories))
                    {
                        catalog.Catalogs.Add(new DirectoryCatalog(subDir, searchPattern));
                    }
                }
                else
                {
                    throw new UserException($"Specified directory '{path}' with modules does not exist");
                }
            }

            return catalog;
        }
    }
}
