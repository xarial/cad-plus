using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;

namespace Xarial.CadPlus.Plus.Modules.QrCode
{
    public interface IDrawingQrCodeModule : IModule
    {
        IQrCodeElement Insert(IXDrawing drw, IXSheet sheet, QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler);
        IQrCodeElement GetQrCode(IXDrawing drw, IXSketchPicture qrCodePicture);
        IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw);
    }
}
