//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.CustomToolbar.Structs
{
    public class ToolbarSettingsVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public ToolbarSettingsVersionTransformer()
        {
        }
    }

    [Settings("Modules\\toolbar-plus.json", "xtoolbar.json")]
    [UserSettingVersion("1.0", typeof(ToolbarSettingsVersionTransformer))]
    public class ToolbarSettings
    {
        public string SpecificationFile { get; set; }

        public ToolbarSettings() 
        {
            SpecificationFile = Path.Combine(Locations.AppDirectoryPath,
                    Settings.Default.ToolbarsSpecFile);
        }

        internal ToolbarSettings Clone()
            => new ToolbarSettings()
            {
                SpecificationFile = SpecificationFile
            };
    }
}