//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Attributes;

namespace Xarial.CadPlus.Plus.Shared
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public static class Licenses
    {
        public static LicenseInfo[] ThirdParty { get; }

        static Licenses()
        {
            ThirdParty = new LicenseInfo[]
            {
                new LicenseInfo()
                {
                    Title = "xCAD.NET",
                    Url = "https://xcad.xarial.com/license/"
                },
                new LicenseInfo()
                {
                    Title = "xToolkit",
                    Url = "https://xtoolkit.xarial.com/license/"
                },
                new LicenseInfo()
                {
                    Title = "SimpleInjector",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "BetterFolderBrowser",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "CommandLineParser",
                    Url = "https://www.nuget.org/packages/CommandLineParser/2.9.1/License"
                },
                new LicenseInfo()
                {
                    Title = "ControlzEx",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "Fluent.Ribbon",
                    Url = "https://www.nuget.org/packages/Fluent.Ribbon/8.0.2/License"
                },
                new LicenseInfo()
                {
                    Title = "MahApps.Metro",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "System.Linq.Dynamic.Core",
                    Url = "https://licenses.nuget.org/Apache-2.0"
                },
                new LicenseInfo()
                {
                    Title = "Newtonsoft.Json",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "Svg",
                    Url = "https://licenses.nuget.org/MS-PL"
                },
                new LicenseInfo()
                {
                    Title = "Polly",
                    Url = "https://licenses.nuget.org/BSD-3-Clause"
                },
                new LicenseInfo()
                {
                    Title = "OpenTK",
                    Url = "https://github.com/opentk/opentk/blob/master/License.txt"
                },
                new LicenseInfo()
                {
                    Title = "ClosedXML",
                    Url = "https://licenses.nuget.org/MIT"
                }
            };
        }
    }
}

