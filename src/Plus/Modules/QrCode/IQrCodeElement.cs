using Xarial.XCad.Sketch;

namespace Xarial.CadPlus.Plus.Modules.QrCode
{
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
}
