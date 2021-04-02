//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Extensions
{
    public static class ImageExtension
    {
        public static byte[] GetBytes(this Image img)
            => GetBytes(img, ImageFormat.Png);

        public static byte[] GetBytes(this Image img, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
