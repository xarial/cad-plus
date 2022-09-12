using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;

namespace Xarial.CadPlus.Plus.Modules
{
    public enum QrCodeDock_e
    {
        [Title("Bottom Left")]
        BottomLeft,

        [Title("Top Left")]
        TopLeft,

        [Title("Top Right")]
        TopRight,

        [Title("Bottom Right")]
        BottomRight
    }

    public delegate void ExpressionSolveErrorHandlerDelegate(Exception err, string expr, out bool cancel);

    public interface IQrCodeElement 
    {
        IXSketchPicture Picture { get; }
        string Expression { get; }
        QrCodeDock_e Dock { get; }
        double Size { get; }
        double OffsetX { get; }
        double OffsetY { get; }

        void Edit(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler);

        /// <summary>
        /// Reloads the QR code and updates the size as per the scale
        /// </summary>
        void Reload(ExpressionSolveErrorHandlerDelegate errHandler);

        /// <summary>
        /// Updates the QR code and keeps current size
        /// </summary>
        void Update(ExpressionSolveErrorHandlerDelegate errHandler);
    }

    public interface IDrawingQrCodeModule : IModule
    {
        IQrCodeElement Insert(IXDrawing drw, IXSheet sheet, QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler);
        IQrCodeElement GetQrCode(IXDrawing drw, IXSketchPicture qrCodePicture);
        IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw);
    }
}
