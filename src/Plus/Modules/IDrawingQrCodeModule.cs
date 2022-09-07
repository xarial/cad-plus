using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;

namespace Xarial.CadPlus.Plus.Modules
{
    public interface IQrCodeInfo 
    {
        void Reload();
        void UpdateInPlace();
    }

    public interface IDrawingQrCodeModule : IModule
    {
        IQrCodeInfo GetQrCode(IXFeature qrCodeFeat);
        IEnumerable<IQrCodeInfo> IterateQrCodes(IXDrawing drw);
    }
}
