using QRCoder;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.CadPlus.Drawing.Services
{
    public class QrCodeManager
    {
        private readonly IXApplication m_App;
        private readonly QrDataProvider m_QrCodeProvider;
        private readonly QRCodeGenerator m_QrGenerator;

        public QrCodeManager(IXApplication app, QrDataProvider dataProvider) 
        {
            m_App = app;
            m_QrCodeProvider = dataProvider;
            m_QrGenerator = new QRCodeGenerator();
        }

        public IXObject Insert(IXDrawing drw, LocationData location, SourceData data)
        {
            var tempFileName = "";

            var model = (drw as ISwDrawing).Model;

            try
            {
                var qrCodeData = m_QrGenerator.CreateQrCode(m_QrCodeProvider.GetData(drw, data),
                    QRCodeGenerator.ECCLevel.Q);

                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, false);

                tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                qrCodeImage.Save(tempFileName);

                var pict = model.SketchManager.InsertSketchPicture(tempFileName);

                if (pict != null)
                {
                    CalculateLocation(drw, location.Dock,
                        location.Size, location.OffsetX,
                        location.OffsetY,
                        out Point centerPt, out double scale);

                    var x = centerPt.X / scale - location.Size / 2;
                    var y = centerPt.Y / scale - location.Size / 2;
                    pict.SetOrigin(x, y);
                    pict.SetSize(location.Size, location.Size, true);

                    //Picture PMPage stays open after inserting the picture
                    const int swCommands_PmOK = -2;
                    (m_App as ISwApplication).Sw.RunCommand(swCommands_PmOK, "");

                    return SwObjectFactory.FromDispatch<ISwObject>(pict, (ISwDocument)drw);
                }
                else
                {
                    throw new UserException("Failed to insert picture");
                }
            }
            finally
            {
                model.IActiveView.EnableGraphicsUpdate = true;

                if (File.Exists(tempFileName))
                {
                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void CalculateLocation(IXDrawing drawing, Dock_e dock,
            double size, double offsetX, double offsetY, out Point centerPt, out double scale)
        {
            var sheet = (drawing.Sheets.Active as ISwSheet).Sheet;
            double sheetWidth = -1;
            double sheetHeight = -1;
            sheet.GetSize(ref sheetWidth, ref sheetHeight);

            var sheetPrps = (double[])sheet.GetProperties2();
            scale = sheetPrps[2] / sheetPrps[3];

            size *= scale;

            double x;
            double y;

            double offsetXDir;
            double offsetYDir;

            switch (dock)
            {
                case Dock_e.BottomLeft:
                    x = size / 2;
                    y = size / 2;
                    offsetXDir = 1;
                    offsetYDir = 1;
                    break;

                case Dock_e.TopLeft:
                    x = size / 2;
                    y = sheetHeight - size / 2;
                    offsetXDir = 1;
                    offsetYDir = -1;
                    break;

                case Dock_e.TopRight:
                    x = sheetWidth - size / 2;
                    y = sheetHeight - size / 2;
                    offsetXDir = -1;
                    offsetYDir = -1;
                    break;

                case Dock_e.BottomRight:
                    x = sheetWidth - size / 2;
                    y = size / 2;
                    offsetXDir = -1;
                    offsetYDir = 1;
                    break;

                default:
                    throw new NotSupportedException();
            }

            centerPt = new Point(x + offsetX * offsetXDir * scale, y + offsetY * offsetYDir * scale, 0);
        }
    }
}
