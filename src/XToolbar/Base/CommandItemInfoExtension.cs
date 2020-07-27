using System.Drawing;
using System.IO;
using Xarial.CadPlus.XToolbar.Properties;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.XToolbar.Base
{
    internal static class CommandItemInfoExtension
    {
        internal static IXImage GetCommandIcon(this CommandItemInfo info)
        {
            Image icon = null;

            try
            {
                if (File.Exists(info.IconPath))
                {
                    icon = Image.FromFile(info.IconPath);
                }
            }
            catch
            {
            }

            if (icon == null)
            {
                icon = Resources.macro_icon_default;
            }

            return new MacroButtonIcon(icon);
        }
    }
}