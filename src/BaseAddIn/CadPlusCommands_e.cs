//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Common.Attributes;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

namespace Xarial.CadPlus.AddIn.Base
{
    [Title("CAD+")]
    [Description("CAD+ Toolset features and options")]
    [CommandGroupInfo((int)CadCommandGroupIds_e.Main)]
    public enum CadPlusCommands_e
    {
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [IconEx(typeof(Resources), nameof(Resources.help_vector), nameof(Resources.help_icon))]
        Help,

        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [IconEx(typeof(Resources), nameof(Resources.about_vector), nameof(Resources.about_icon))]
        About
    }
}
