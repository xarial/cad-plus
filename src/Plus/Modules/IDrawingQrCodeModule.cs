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
        IQrCodeElement GetQrCode(IXDrawing drw, IXSketchPicture qrCodePicture);
        IEnumerable<IQrCodeElement> IterateQrCodes(IXDrawing drw);
    }
}
