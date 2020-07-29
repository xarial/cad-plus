//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.XToolbar.Enums;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.XToolbar.Base
{
    internal class CommandItemInfoSpec : CommandSpec
    {
        internal CommandMacroInfo Info { get; }

        internal CommandItemInfoSpec(CommandMacroInfo info)
        {
            Info = info;

            UserId = info.Id;
            Title = info.Title;
            Tooltip = info.Description;
            Icon = info.GetCommandIcon();
            HasToolbar = info.Location.HasFlag(Location_e.Toolbar);
            HasMenu = info.Location.HasFlag(Location_e.Menu);
            HasTabBox = info.Location.HasFlag(Location_e.TabBox);
            if (HasTabBox) 
            {
                TabBoxStyle = XCad.UI.Commands.Enums.RibbonTabTextDisplay_e.TextBelow;
            }
        }
    }
}