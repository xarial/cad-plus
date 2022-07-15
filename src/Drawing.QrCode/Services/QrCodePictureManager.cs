//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using QRCoder;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.CadPlus.Drawing.QrCode.Services
{
    public class QrCodePictureManager
    {
        private class ViewFreeze : IDisposable
        {
            private readonly IModelDoc2 m_Model;

            internal ViewFreeze(IXDocument doc)
            {
                m_Model = ((ISwDocument)doc).Model;

                m_Model.FeatureManager.EnableFeatureTree = false;
                m_Model.FeatureManager.EnableFeatureTreeWindow = false;
                m_Model.IActiveView.EnableGraphicsUpdate = false;
            }

            public void Dispose()
            {
                m_Model.FeatureManager.EnableFeatureTree = true;
                m_Model.FeatureManager.EnableFeatureTreeWindow = true;
                m_Model.IActiveView.EnableGraphicsUpdate = true;
            }
        }

        private readonly IXApplication m_App;
        private readonly QrDataProvider m_QrCodeProvider;
        private readonly QRCodeGenerator m_QrGenerator;

        public QrCodePictureManager(IXApplication app, QrDataProvider dataProvider)
        {
            m_App = app;
            m_QrCodeProvider = dataProvider;
            m_QrGenerator = new QRCodeGenerator();
        }

        public IXObject Insert(IXDrawing drw, LocationData location, SourceData data)
        {
            using (var freeze = new ViewFreeze(drw))
            {
                return CalculateLocationAndInsert(drw, location, data);
            }
        }

        public IXObject Reload(IXObject pict, LocationData location, SourceData data, IXDrawing drw)
        {
            using (var freeze = new ViewFreeze(drw))
            {
                DeletePicture(pict, drw);
                return CalculateLocationAndInsert(drw, location, data);
            }
        }

        public IXObject UpdateInPlace(IXObject pict, SourceData data, IXDrawing drw)
        {
            var skPict = (ISketchPicture)((ISwObject)pict).Dispatch;

            double width = -1;
            double height = -1;
            double x = -1;
            double y = -1;

            skPict.GetSize(ref width, ref height);
            skPict.GetOrigin(ref x, ref y);

            using (var freeze = new ViewFreeze(drw))
            {
                DeletePicture(pict, drw);

                return InsertAt(drw, data, width, height, x, y);
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

            centerPt = new Point(x + offsetX * offsetXDir, y + offsetY * offsetYDir, 0);
        }

        private void DeletePicture(IXObject pict, IXDocument doc)
        {
            var skPict = (ISketchPicture)((ISwObject)pict).Dispatch;

            if (skPict.GetFeature().Select2(false, -1))
            {
                if (!((ISwDocument)doc).Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed))
                {
                    throw new UserException("Failed to update QR code", new Exception("Failed to remove sketch picture"));
                }
            }
            else
            {
                throw new UserException("Failed to update QR code", new Exception("Failed to select sketch picture"));
            }
        }

        private IXObject CalculateLocationAndInsert(IXDrawing drw, LocationData location, SourceData data)
        {
            CalculateLocation(drw, location.Dock,
                            location.Size, location.OffsetX,
                            location.OffsetY,
                            out Point centerPt, out double scale);

            var x = (centerPt.X - location.Size / 2) / scale;
            var y = (centerPt.Y - location.Size / 2) / scale;

            return InsertAt(drw, data, location.Size / scale, location.Size / scale, x, y);
        }

        private IXObject InsertAt(IXDrawing drw, SourceData data, double width, double height, double origX, double origY)
        {
            var tempFileName = "";

            try
            {
                var model = (drw as ISwDrawing).Model;

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

                    //picture PMPage stays open after inserting the picture
                    const int swCommands_PmOK = -2;
                    (m_App as ISwApplication).Sw.RunCommand(swCommands_PmOK, "");

                    return ((ISwDocument)drw).CreateObjectFromDispatch<ISwObject>(pict);
                }
                else
                {
                    throw new UserException("Failed to insert picture");
                }
            }
            finally
            {
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
