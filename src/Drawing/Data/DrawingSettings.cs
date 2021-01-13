using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.Drawing.Data
{
    public class DrawingSettingsVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public DrawingSettingsVersionTransformer()
        {
        }
    }

    [Settings("Modules\\drawing-plus.json")]
    [UserSettingVersion("1.0", typeof(DrawingSettingsVersionTransformer))]
    public class DrawingSettings
    {
        public string PdmWeb2Server { get; set; }
    }
}
