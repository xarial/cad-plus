using System;
using System.Linq;
using Xarial.CadPlus.XToolbar.Enums;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.XToolbar.Base
{
    internal class CommandGroupInfoSpec : CommandGroupSpec
    {
        internal CommandGroupInfoSpec(CommandGroupInfo info)
        {
            Id = info.Id;
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