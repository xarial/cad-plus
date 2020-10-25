//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    internal class CommandItemInfoSpec : CommandSpec
    {
        internal CommandMacroInfo Info { get; }

        internal CommandItemInfoSpec(CommandMacroInfo info) : base(info.Id)
        {
            Info = info;

            Title = info.Title;
            Tooltip = info.Description;
            Icon = info.GetCommandIcon();
            HasToolbar = info.Location.HasFlag(Location_e.Toolbar);
            HasMenu = info.Location.HasFlag(Location_e.Menu);
            //HasTabBox = info.Location.HasFlag(Location_e.TabBox);
            //if (HasTabBox) 
            //{
            //    TabBoxStyle = XCad.UI.Commands.Enums.RibbonTabTextDisplay_e.TextBelow;
            //}
        }
    }
}