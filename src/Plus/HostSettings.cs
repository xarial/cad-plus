//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.Plus
{
    public class HostSettingsVersionTransformer : IVersionsTransformer
    {
        public IReadOnlyList<VersionTransform> Transforms { get; }

        public HostSettingsVersionTransformer()
        {
            Transforms = new VersionTransform[] 
            {
            };
        }
    }

    [Settings("host.json")]
    [UserSettingVersion("1.0", typeof(HostSettingsVersionTransformer))]
    public class HostSettings
    {
        public string[] AdditionalModuleFolders { get; set; }
    }
}
