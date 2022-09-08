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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Shared.Data;
using Xarial.XCad;
using Xarial.XCad.Base;
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
        private readonly QrDataProvider m_QrCodeProvider;
        private readonly QRCodeGenerator m_QrGenerator;

        public QrCodePictureManager(QrDataProvider dataProvider)
        {
            m_QrCodeProvider = dataProvider;
            m_QrGenerator = new QRCodeGenerator();
        }

        public IXSketchPicture Insert(IXDrawing drw, LocationData location, SourceData data)
            => CalculateLocationAndInsert(drw, location, data);

        public IXSketchPicture Reload(IXSketchPicture pict, LocationData location, SourceData data, IXDrawing drw)
        {
            drw.Features.Remove(pict);
            return CalculateLocationAndInsert(drw, location, data);
        }

        public IXSketchPicture UpdateInPlace(IXSketchPicture pict, SourceData data, IXDrawing drw)
        {
            var bounds = pict.Boundary;

            drw.Features.Remove(pict);

            return InsertAt(drw, data, bounds.Width, bounds.Height, bounds.CenterPoint.X, bounds.CenterPoint.Y);
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

        private IXSketchPicture CalculateLocationAndInsert(IXDrawing drw, LocationData location, SourceData data)
        {
            CalculateLocation(drw, location.Dock,
                            location.Size, location.OffsetX,
                            location.OffsetY,
                            out Point centerPt, out double scale);

            var x = centerPt.X / scale;
            var y = centerPt.Y / scale;

            return InsertAt(drw, data, location.Size / scale, location.Size / scale, x, y);
        }

        private IXSketchPicture InsertAt(IXDrawing drw, SourceData data, double width, double height, double origX, double origY)
        {
            var qrCodeData = m_QrGenerator.CreateQrCode(m_QrCodeProvider.GetData(drw, data),
                    QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, false);

            try
            {
                var pict = drw.Features.PreCreate<IXSketchPicture>();
                
                pict.Image = new XDrawingImage(qrCodeImage, ImageFormat.Png);
                pict.Boundary = new Rect2D(width, height, new Point(origX, origY, 0));
                pict.Commit();

                return pict;
            }
            catch (Exception ex) 
            {
                throw new UserException("Failed to insert picture", ex);
            }
        }
    }
}
