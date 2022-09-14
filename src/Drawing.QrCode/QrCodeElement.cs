using QRCoder;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Shared.Data;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.CadPlus.Drawing.QrCode
{
    public class QrCodeElement : IQrCodeElement
    {
        public static QrCodeElement FromSketchPicture(IXSketchPicture pict, IXDrawing drw, IXApplication app, QrDataProvider dataProvider, IXLogger logger)
        {
            var handler = app.Documents.GetHandler<QrCodeDrawingHandler>(drw);
            var qrCodeInfo = handler.QrCodes.FirstOrDefault(d => d.Picture.Equals(pict));

            if (qrCodeInfo == null)
            {
                throw new UserException("This picture does not contain QR code data");
            }

            return new QrCodeElement(qrCodeInfo, app, drw, dataProvider, logger);
        }

        public static void CalculateLocation(IXSheet sheet, QrCodeDock_e dock,
            double size, double offsetX, double offsetY, out Point centerPt)
        {
            var paperSize = sheet.PaperSize;
            var sheetHeight = paperSize.Height.Value;
            var sheetWidth = paperSize.Width.Value;

            double x;
            double y;

            double offsetXDir;
            double offsetYDir;

            switch (dock)
            {
                case QrCodeDock_e.BottomLeft:
                    x = size / 2;
                    y = size / 2;
                    offsetXDir = 1;
                    offsetYDir = 1;
                    break;

                case QrCodeDock_e.TopLeft:
                    x = size / 2;
                    y = sheetHeight - size / 2;
                    offsetXDir = 1;
                    offsetYDir = -1;
                    break;

                case QrCodeDock_e.TopRight:
                    x = sheetWidth - size / 2;
                    y = sheetHeight - size / 2;
                    offsetXDir = -1;
                    offsetYDir = -1;
                    break;

                case QrCodeDock_e.BottomRight:
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

        private static IXSheet GetSheet(IXDrawing drw, IXSketchPicture pict)
        {
            var ownerSketch = pict.OwnerSketch;
            var sheet = drw.Sheets.FirstOrDefault(s => s.Sketch.Equals(ownerSketch));

            if (sheet == null)
            {
                throw new NullReferenceException($"Failed to find the sheet of '{pict.Name}'");
            }

            return sheet;
        }

        public QrCodeInfo Info { get; private set; }

        public IXDrawing Drawing { get; }
        public IXSheet Sheet { get; }

        public IXSketchPicture Picture => Info.Picture;

        public string Expression => Info.Expression;

        public QrCodeDock_e Dock => Info.Dock;

        public double Size => Info.Size;

        public double OffsetX => Info.OffsetX;

        public double OffsetY => Info.OffsetY;

        private readonly IXApplication m_App;

        private readonly QrDataProvider m_QrCodeProvider;
        private readonly QRCodeGenerator m_QrGenerator;

        private Tuple<int, double, int, double> m_PictTrans;

        private readonly IXLogger m_Logger;

        public QrCodeElement(IXApplication app, IXDrawing drw, IXSheet sheet, QrDataProvider dataProvider, IXLogger logger)
        {
            m_App = app;
            Drawing = drw;
            Sheet = sheet;

            m_QrCodeProvider = dataProvider;
            m_Logger = logger;

            m_QrGenerator = new QRCodeGenerator();
        }

        public QrCodeElement(QrCodeInfo info, IXApplication app, IXDrawing drw, QrDataProvider dataProvider, IXLogger logger) 
            : this(app, drw, GetSheet(drw, info.Picture), dataProvider, logger)
        {
            Info = info;
        }

        public void Create(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler) 
        {
            if (Info == null)
            {
                try
                {
                    var pict = CalculateLocationAndInsert(dock, size, offsetX, offsetY, expression, errHandler);

                    Info = new QrCodeInfo()
                    {
                        Picture = pict,
                        Expression = expression,
                        Dock = dock,
                        Size = size,
                        OffsetX = offsetX,
                        OffsetY = offsetY
                    };

                    var handler = m_App.Documents.GetHandler<QrCodeDrawingHandler>(Drawing);

                    handler.QrCodes.Add(Info);
                }
                catch (OperationCanceledException)
                {
                    m_Logger.Log("QR code creation is cancelled by the user", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
                    return;
                }
            }
            else 
            {
                throw new Exception("Create is only possible to the new qr code element");
            }
        }

        public void Reload(ExpressionSolveErrorHandlerDelegate errHandler)
        {
            if (Info != null)
            {
                Edit(Info.Dock, Info.Size, Info.OffsetX, Info.OffsetY, Info.Expression, errHandler);
            }
            else
            {
                throw new Exception("Reload is only possible for the created qr code element");
            }
        }

        public void Edit(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler)
        {
            if (Info != null)
            {
                try
                {
                    var pict = Info.Picture;

                    Info.Picture = CalculateLocationAndInsert(dock, size, offsetX, offsetY, expression, errHandler);

                    Drawing.Features.Remove(pict);

                    Info.Expression = expression;
                    Info.Dock = dock;
                    Info.Size = size;
                    Info.OffsetX = offsetX;
                    Info.OffsetY = offsetY;
                }
                catch (OperationCanceledException)
                {
                    m_Logger.Log("QR code editing is cancelled by the user", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
                    return;
                }
            }
            else
            {
                throw new Exception("Updated is only possible for the created qr code element");
            }
        }

        public void Update(ExpressionSolveErrorHandlerDelegate errHandler)
        {
            if (Info != null)
            {
                var pict = Info.Picture;

                var bounds = pict.Boundary;

                try
                {
                    Info.Picture = InsertAt(Info.Expression, bounds.Width, bounds.Height, bounds.CenterPoint.X, bounds.CenterPoint.Y, errHandler);
                    Drawing.Features.Remove(pict);
                }
                catch (OperationCanceledException) 
                {
                    m_Logger.Log("QR code updating is cancelled by the user", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
                    return;
                }
            }
            else
            {
                throw new Exception("Updated in place is only possible for the created qr code element");
            }
        }

        public void Show() => ChangeVisibility(false);

        public void Hide() => ChangeVisibility(true);

        private IXSketchPicture CalculateLocationAndInsert(QrCodeDock_e dock, double size, double offsetX, double offsetY, string expression, ExpressionSolveErrorHandlerDelegate errHandler)
        {
            CalculateLocation(Sheet, dock, size, offsetX, offsetY, out Point centerPt);

            var scale = Sheet.Scale.AsDouble();

            var x = centerPt.X / scale;
            var y = centerPt.Y / scale;

            return InsertAt(expression, size / scale, size / scale, x, y, errHandler);
        }

        private IXSketchPicture InsertAt(string expression, double width, double height, double origX, double origY, ExpressionSolveErrorHandlerDelegate errHandler)
        {
            System.Drawing.Bitmap qrCodeImage;

            try
            {
                var qrCodeText = m_QrCodeProvider.GetData(Drawing, expression);

                qrCodeImage = GetQrCodeImage(qrCodeText);
            }
            catch (Exception ex)
            {
                errHandler.Invoke(ex, expression, out var cancel);

                if (cancel)
                {
                    throw new OperationCanceledException();
                }
                else 
                {
                    qrCodeImage = CreateErrorQrCode(width, height);
                }
            }

            try
            {
                var pict = Sheet.Sketch.Entities.PreCreate<IXSketchPicture>();
                pict.Image = new XDrawingImage(qrCodeImage, ImageFormat.Png);
                pict.Boundary = new Rect2D(width, height, new Point(origX, origY, 0));
                pict.Commit();

                return pict;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                throw new UserException("Failed to insert picture", ex);
            }
        }

        private System.Drawing.Bitmap GetQrCodeImage(string qrCodeText)
        {
            System.Drawing.Bitmap qrCodeImage;
            var qrCodeData = m_QrGenerator.CreateQrCode(qrCodeText, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);
            qrCodeImage = qrCode.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, false);
            return qrCodeImage;
        }

        private System.Drawing.Bitmap CreateErrorQrCode(double width, double height) 
        {
            const string ERR_QR_CODE_TEXT = "!";

            var bmp = GetQrCodeImage(ERR_QR_CODE_TEXT);

            using (var graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);

                var lineWidth = Math.Min(bmp.Width, bmp.Height) * 0.025f;

                if (lineWidth < 1) 
                {
                    lineWidth = 1;
                }

                var pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, lineWidth);
                
                graphics.DrawLine(pen, new System.Drawing.Point(rect.Left, rect.Top), new System.Drawing.Point(rect.Right, rect.Bottom));
                graphics.DrawLine(pen, new System.Drawing.Point(rect.Left, rect.Bottom), new System.Drawing.Point(rect.Right, rect.Top));

                return bmp;
            }
        }

        private void ChangeVisibility(bool hide)
        {
            if (Info != null)
            {
                const int OPAQUE = 0;
                const int TRANSPARENT = 1;
                const int COLOR_IGNORE = 0;
                const int COLOR_EXACT_MATCH = 0;

                var skPict = ((ISwSketchPicture)Info.Picture).SketchPicture;

                if (hide)
                {
                    int style = -1;
                    double trans = -1;
                    int matchColor = -1;
                    double matchTol = -1;

                    skPict.GetTransparency(ref style, ref trans, ref matchColor, ref matchTol);
                    m_PictTrans = new Tuple<int, double, int, double>(style, trans, matchColor, matchTol);

                    skPict.SetTransparency((int)swSketchPictureTransparencyStyle_e.swSketchPictureTransparencyFullImage,
                        TRANSPARENT, COLOR_IGNORE, COLOR_EXACT_MATCH);
                }
                else
                {
                    var style = m_PictTrans?.Item1 ?? (int)swSketchPictureTransparencyStyle_e.swSketchPictureTransparencyFullImage;
                    var trans = m_PictTrans?.Item2 ?? OPAQUE;
                    int matchColor = m_PictTrans?.Item3 ?? COLOR_IGNORE;
                    double matchTol = m_PictTrans?.Item4 ?? COLOR_EXACT_MATCH;

                    skPict.SetTransparency(style, trans, matchColor, matchTol);
                }
            }
            else 
            {
                throw new Exception("Visibility can only be changed for created qr code element");
            }
        }
    }
}
