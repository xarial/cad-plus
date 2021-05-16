using QRCoder;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
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
            CalculateLocation(drw, location.Dock,
                location.Size, location.OffsetX,
                location.OffsetY,
                out Point centerPt, out double scale);

            var x = centerPt.X / scale - location.Size / 2;
            var y = centerPt.Y / scale - location.Size / 2;

            return InsertAt(drw, data, location.Size, location.Size, x, y);
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

        public void Update(IXObject pict, IXDrawing drw) 
        {
            var handler = m_App.Documents.GetHandler<QrCodeDrawingHandler>(drw);
            var qrCode = handler.QrCodes.FirstOrDefault(d => d.Picture.Equals(pict));

            if (qrCode == null)
            {
                throw new UserException("This picture does not contain QR code data");
            }

            var skPict = (ISketchPicture)((ISwObject)pict).Dispatch;

            var data = GetSourceData(qrCode);

            double width = -1;
            double height = -1;
            double x = -1;
            double y = -1;

            skPict.GetSize(ref width, ref height);
            skPict.GetOrigin(ref x, ref y);

            (drw as ISwDrawing).Model.IActiveView.EnableGraphicsUpdate = false;

            try
            {
                if (skPict.GetFeature().Select2(false, -1))
                {
                    if (((ISwDrawing)drw).Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed))
                    {
                        qrCode.Picture = InsertAt(drw, data, width, height, x, y);
                    }
                    else
                    {
                        throw new UserException("Failed to update QR code", new Exception("Failed to remove sketch picture"));
                    }
                }
                else
                {
                    throw new UserException("Failed to update QR code", new Exception("Failed to select sketch picture"));
                }
            }
            finally 
            {
                (drw as ISwDrawing).Model.IActiveView.EnableGraphicsUpdate = true;
            }
        }

        private SourceData GetSourceData(QrCodeData data) 
        {
            return new SourceData()
            {
                Source = data.Source,
                ReferencedDocument = data.RefDocumentSource,
                CustomPropertyName = data.Source == Source_e.CustomProperty ? data.Argument : "",
                PdmWeb2Server = data.Source == Source_e.PdmWeb2Url ? data.Argument : "",
                CustomValue = data.Source == Source_e.Custom ? data.Argument : ""
            };
        }

        private IXObject InsertAt(IXDrawing drw, SourceData data, double width, double height, double origX, double origY)
        {
            var tempFileName = "";

            var model = (drw as ISwDrawing).Model;

            model.IActiveView.EnableGraphicsUpdate = false;

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
                    pict.SetOrigin(origX, origY);
                    pict.SetSize(width, height, true);

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
    }
}
