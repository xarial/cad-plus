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

    public interface IQrCodeElement 
    {
        IXSketchPicture Picture { get; }
        string Expression { get; }
        QrCodeDock_e Dock { get; }
        double Size { get; }
        double OffsetX { get; }
        double OffsetY { get; }

        void Edit(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression);

        /// <summary>
        /// Reloads the QR code and updates the size as per the scale
        /// </summary>
        void Reload();

        /// <summary>
        /// Updates the QR code and keeps current size
        /// </summary>
        void Update();
    }

    public interface IDrawingQrCodeModule : IModule
    {
        IQrCodeElement Insert(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression);
        IQrCodeElement GetQrCode(IXDrawing drw, IXSketchPicture qrCodePicture);
        IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw);
    }
}
