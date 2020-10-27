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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Sw
{
    public class SwApplicationProvider : IApplicationProvider
    {
        public FileFilter[] InputFilesFilter { get; }

        public FileFilter[] MacroFilesFilter { get; }

        public SwApplicationProvider()
        {
            InputFilesFilter = new FileFilter[]
            {
                new FileFilter("SOLIDWORKS Parts", "*.sldprt"),
                new FileFilter("SOLIDWORKS Assemblies", "*.sldasm"),
                new FileFilter("SOLIDWORKS Drawings", "*.slddrw"),
                new FileFilter("SOLIDWORKS Files", "*.sldprt", "*.sldasm", "*.slddrw"),
                FileFilter.AllFiles
            };

            MacroFilesFilter = new FileFilter[]
            {
                new FileFilter("VBA Macros", "*.swp"),
                new FileFilter("SWBasic Macros", "*.swb"),
                new FileFilter("VSTA Macros", "*.dll"),
                new FileFilter("All Macros", "*.swp", "*.swb", "*.dll"),
                FileFilter.AllFiles
            };
        }

        public IEnumerable<AppVersionInfo> GetInstalledVersions() 
            => SwApplication.GetInstalledVersions()
            .Select(x => new SwAppVersionInfo(x));

        public AppVersionInfo ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                var installedVers = SwApplication.GetInstalledVersions();
                if (installedVers.Any())
                {
                    return new SwAppVersionInfo(installedVers.OrderBy(v => (int)v).First());
                }
                else
                {
                    throw new Exception("Failed to find installed version of the host application");
                }
            }
            else if (int.TryParse(version, out int rev))
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), $"Sw{rev}");
                return new SwAppVersionInfo(swVers);
            }
            else if (version.StartsWith("solidworks", StringComparison.CurrentCultureIgnoreCase))
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), $"Sw{version.Substring("solidworks".Length).Trim()}");
                return new SwAppVersionInfo(swVers);
            }
            else 
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), version);
                return new SwAppVersionInfo(swVers);
            }
        }

        public IXApplication StartApplication(AppVersionInfo vers, StartupOptions_e opts, CancellationToken cancellationToken)
        {
            var args = new List<string>();

            if (opts.HasFlag(StartupOptions_e.Safe))
            {
                args.Add(SwApplication.CommandLineArguments.SafeMode);
            }

            if (opts.HasFlag(StartupOptions_e.Background))
            {
                args.Add(SwApplication.CommandLineArguments.BackgroundMode);
            }

            if (opts.HasFlag(StartupOptions_e.Silent))
            {
                args.Add(SwApplication.CommandLineArguments.SilentMode);
            }

            var app = SwApplication.Start(((SwAppVersionInfo)vers).Version,
                  string.Join(" ", args),
                  cancellationToken);
            
            app.Sw.CommandInProgress = true;

            return app;
        }

        public bool CanProcessFile(string filePath)
        {
            const string TEMP_SW_FILE_NAME = "~$";

            var fileName = Path.GetFileName(filePath);

            return !fileName.StartsWith(TEMP_SW_FILE_NAME);
        }
    }
}
