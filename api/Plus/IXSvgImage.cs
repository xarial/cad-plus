using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus
{
    public interface IXSvgImage : IXImage
    {
        byte[] SvgBuffer { get; }
    }
}
