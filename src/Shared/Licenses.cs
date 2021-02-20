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
                    Title = "Autofac",
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
                    Url = "https://www.nuget.org/packages/CommandLineParser/2.8.0/License"
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
                    Title = "Microsoft.CodeAnalysis.CSharp",
                    Url = "https://licenses.nuget.org/MIT"
                },
                new LicenseInfo()
                {
                    Title = "Microsoft.CodeAnalysis.VisualBasic",
                    Url = "https://licenses.nuget.org/MIT"
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
                }
            };
        }
    }
}

