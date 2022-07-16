//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    public enum Dock_e
    {
        [Title("Bottom Left")]
        BottomLeft,

        [Title("Top Left")]
        TopLeft,

        [Title("Top Right")]
        TopRight,

        [Title("Bottom Right")]
        BottomRight
    }
}
