using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.Common
{
    public class HostSettingsVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public HostSettingsVersionTransformer()
        {
        }
    }

    [Settings("host.json")]
    [UserSettingVersion("1.0", typeof(HostSettingsVersionTransformer))]
    public class HostSettings
    {
        public string[] AdditionalModuleFolders { get; set; }
    }
}
