using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus.Shared.Data
{
    public class XDrawingImage : IXImage
    {
        public byte[] Buffer { get; }

        public XDrawingImage(Image icon)
        {
            ImageFormat format = null;

            try
            {
                format = icon.RawFormat;
            }
            catch 
            {
            }

            if (format == null) 
            {
                format = ImageFormat.Png;
            }

            Buffer = icon.GetBytes(format);
        }
    }
}
