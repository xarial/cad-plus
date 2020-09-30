//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.CadPlus.XBatch.Sw
{
    public class SwApplicationProvider : IApplicationProvider
    {
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
                return new SwAppVersionInfo((SwVersion_e)rev);
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

        public IXApplication StartApplication(AppVersionInfo vers, bool background, CancellationToken cancellationToken)
            => SwApplication.Start(((SwAppVersionInfo)vers).Version, 
                background ? "/b" : "",
                cancellationToken);
    }
}
