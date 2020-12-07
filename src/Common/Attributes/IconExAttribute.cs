using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Common.Attributes
{
    public class IconExAttribute : IconAttribute
    {
        public override IXImage Icon { get; }

        public IconExAttribute(Type resType, string svgResName, string imgResName)
        {
            var img = ResourceHelper.GetResource<Image>(resType, imgResName);

            byte[] imgBuffer = null;

            using (var ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                imgBuffer = ms.ToArray();
            }

            var svgBuffer = ResourceHelper.GetResource<byte[]>(resType, svgResName);

            Icon = new ImageEx(imgBuffer, svgBuffer);
        }
    }
}
