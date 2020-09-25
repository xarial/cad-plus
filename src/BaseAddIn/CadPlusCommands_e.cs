//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.AddIn.Base.Properties;
using Xarial.CadPlus.ExtensionModule;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

namespace Xarial.CadPlus.AddIn.Base
{
    [CommandGroupInfo(CommandGroups.RootGroupId)]
    [Title("CAD+ Toolset")]
    [Description("CAD+ Toolset features and options")]
    public enum CadPlusCommands_e
    {
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [Icon(typeof(Resources), nameof(Resources.help_icon))]
        Help,

        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [Icon(typeof(Resources), nameof(Resources.about_icon))]
        About
    }
}
