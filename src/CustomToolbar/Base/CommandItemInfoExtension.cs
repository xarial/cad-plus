//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using System.IO;
using System.Linq;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    internal static class CommandItemInfoExtension
    {
        internal static IXImage GetCommandIcon(this CommandItemInfo info, IIconsProvider[] iconsProviders)
        {
            IXImage icon = null;

            try
            {
                var provider = iconsProviders.FirstOrDefault(p => p.Matches(info.IconPath));

                if (provider != null) 
                {
                    icon = provider.GetIcon(info.IconPath);
                }
            }
            catch
            {
            }

            if (icon == null)
            {
                if (info is CommandMacroInfo)
                {
                    icon = new ImageIcon(Resources.macro_icon_default);
                }
                else if (info is CommandGroupInfo) 
                {
                    icon = new ImageIcon(Resources.group_icon_default);
                }
            }

            return icon;
        }
    }
}