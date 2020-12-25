//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Common
{
    public class ImageEx : IXSvgImage
    {
        public byte[] SvgBuffer { get; }
        public byte[] Buffer { get; }

        public ImageEx(byte[] imgBuffer, byte[] svgBuffer)
        {
            Buffer = imgBuffer;
            SvgBuffer = svgBuffer;
        }
    }
}
