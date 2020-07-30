using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.CustomToolbar.Enums
{
    [Flags]
    public enum Location_e
    {
        [Summary("Create command in toolbar")]
        [Title("Toolbar")]
        Toolbar = 1 << 0,

        [Summary("Create command in menu")]
        [Title("Menu")]
        Menu = 1 << 1,

        [Summary("Create command in command tab box")]
        [Title("Tab Box")]
        TabBox = 1 << 2
    } 
}
