//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    internal class CommandGroupInfoSpec : CommandGroupSpec
    {
        internal CommandGroupInfoSpec(CommandGroupInfo info) : base(info.Id)
        {
            Title = info.Title;
            Tooltip = info.Description;
            Icon = info.GetCommandIcon();

            if (info.Commands != null)
            {
                Commands = info.Commands.Where(c => c.Triggers.HasFlag(Triggers_e.Button)).Select(
                    c => new CommandItemInfoSpec(c)).ToArray();
            }
            else
            {
                Commands = new CommandItemInfoSpec[0];
            }
        }
    }
}