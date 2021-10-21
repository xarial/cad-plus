//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Common;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    internal static class CommandItemInfoExtension
    {
        internal static IXImage GetCommandIcon(this CommandItemInfo info, IIconsProvider[] iconsProviders, IFilePathResolver pathResolver, string workDir)
        {
            IXImage icon = null;

            try
            {
                if (!string.IsNullOrEmpty(info.IconPath))
                {
                    var iconPath = pathResolver.Resolve(info.IconPath, workDir);

                    var provider = iconsProviders.FirstOrDefault(p => p.Matches(iconPath));

                    if (provider != null)
                    {
                        icon = provider.GetIcon(iconPath);
                    }
                }
            }
            catch
            {
            }

            if (icon == null)
            {
                if (info is CommandMacroInfo)
                {
                    icon = new ImageEx(Resources.macro_icon_default.GetBytes(), 
                        Resources.macro_vector);
                }
                else if (info is CommandGroupInfo) 
                {
                    icon = new ImageEx(Resources.group_icon_default.GetBytes(),
                        Resources.macros_vector);
                }
            }

            return icon;
        }
    }
}