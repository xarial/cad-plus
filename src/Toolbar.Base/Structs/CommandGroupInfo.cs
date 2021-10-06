//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;

namespace Xarial.CadPlus.CustomToolbar.Structs
{
    public class CommandGroupInfo : CommandItemInfo
    {
        public CommandMacroInfo[] Commands { get; set; }

        internal CommandGroupInfo Clone()
            => new CommandGroupInfo()
            {
                Description = Description,
                IconPath = IconPath,
                Id = Id,
                Title = Title,
                Commands = Commands?.Select(c => c.Clone()).ToArray(),
            };
    }
}