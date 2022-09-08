using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;

namespace Xarial.CadPlus.Plus.Modules
{
    public interface IQrCodeElement 
    {
        void Reload();
        void UpdateInPlace();
    }

    public interface IDrawingQrCodeModule : IModule
    {
        IQrCodeElement GetQrCode(IXFeature qrCodeFeat);
        IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw);
    }
}
