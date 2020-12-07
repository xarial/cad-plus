//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using System.IO;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    public class ImageIcon : IXImage
    {
        public byte[] Buffer { get; }

        internal ImageIcon(Image icon)
        {
            Buffer = ImageToByteArray(icon);
        }

        internal static byte[] ImageToByteArray(Image img) 
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                return ms.ToArray();
            }
        }
    }
}
