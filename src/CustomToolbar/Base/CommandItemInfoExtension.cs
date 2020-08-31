//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using System.IO;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Base
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
                if (info is CommandMacroInfo)
                {
                    icon = Resources.macro_icon_default;
                }
                else if (info is CommandGroupInfo) 
                {
                    icon = Resources.group_icon_default;
                }
            }

            return new MacroButtonIcon(icon);
        }
    }
}