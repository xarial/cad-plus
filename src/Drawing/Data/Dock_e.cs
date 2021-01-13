using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Drawing.Data
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
