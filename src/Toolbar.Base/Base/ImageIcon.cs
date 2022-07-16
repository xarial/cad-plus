//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using System.IO;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    public class ImageIcon : IXImage
    {
        public byte[] Buffer { get; }

        internal ImageIcon(Image icon)
        {
            Buffer = icon.GetBytes(icon.RawFormat);
        }
    }
}
