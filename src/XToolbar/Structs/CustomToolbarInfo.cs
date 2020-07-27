using System;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.XToolbar.Structs
{
    public class CustomToolbarInfoVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public CustomToolbarInfoVersionTransformer()
        {
            Add(new Version("1.0.0"), new Version("2.0.0"), t => t);
        }
    }

    [UserSettingVersion("2.0", typeof(CustomToolbarInfoVersionTransformer))]
    public class CustomToolbarInfo
    {
        public CommandGroupInfo[] Groups { get; set; }

        public CustomToolbarInfo()
        {
        }
    }
}