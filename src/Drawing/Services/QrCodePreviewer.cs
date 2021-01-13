using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using static Xarial.CadPlus.Drawing.OpenGL;

namespace Xarial.CadPlus.Drawing.Services
{
    public class QrCodePreviewer : IDisposable
    {
        private IXDrawing m_Drw;
        private ModelView m_View;

        private Dock_e m_Dock;
        private double m_Size;
        private double m_OffsetX;
        private double m_OffsetY;

        public QrCodePreviewer(IXDrawing drw)
        {
            m_Drw = drw;
            m_View = ((ISwDrawing)m_Drw).Model.IActiveView;
            m_View.BufferSwapNotify += OnBufferSwapNotify;
        }

        public void Preview(byte[] data, Dock_e dock, double size, double offsetX, double offsetY) 
        {
            m_Dock = dock;
            m_Size = size;
            m_OffsetX = offsetX;
            m_OffsetY = offsetY;
            ((ISwDrawing)m_Drw).Model.GraphicsRedraw2();
        }

        private int OnBufferSwapNotify()
        {
            CalculateLocation(m_Drw, m_Dock, m_Size, m_OffsetX, m_OffsetY, out Point centerPt, out double scale);
            RenderQrCodeTemplate(centerPt, m_Size * scale);
            return 0;
        }

        public void Dispose()
        {
            m_View.BufferSwapNotify -= OnBufferSwapNotify;
            ((ISwDrawing)m_Drw).Model.GraphicsRedraw2();
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

            double x = 0;
            double y = 0;

            double offsetXDir = 1;
            double offsetYDir = 1;

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

        private void RenderQrCodeTemplate(Point centerPt, double size)
        {
            glDisable(GL_LIGHTING);
            glEnable(GL_BLEND);

            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            RenderRectangle2D(centerPt, size, size, System.Drawing.Color.White, true);
            RenderRectangle2D(centerPt, size, size, System.Drawing.Color.Black, false);

            var markerCenterPts = new Point[]
            {
                new Point(centerPt.X - size / 3, centerPt.Y - size / 3, 0),
                new Point(centerPt.X - size / 3, centerPt.Y + size / 3, 0),
                new Point(centerPt.X + size / 3, centerPt.Y + size / 3, 0)
            };

            var markerWidth = size / 3;

            foreach (var markerCenterPt in markerCenterPts)
            {
                RenderRectangle2D(markerCenterPt, markerWidth, markerWidth, System.Drawing.Color.Black, true);
                RenderRectangle2D(markerCenterPt, markerWidth * 2 / 3, markerWidth * 2 / 3, System.Drawing.Color.White, true);
                RenderRectangle2D(markerCenterPt, markerWidth / 3, markerWidth / 3, System.Drawing.Color.Black, true);
            }

            glEnd();
        }

        private static void RenderRectangle2D(Point center, double width, double height,
            System.Drawing.Color color, bool fill)
        {
            RenderPolygon(new Point[]
            {
                new Point(center.X - width / 2, center.Y - height / 2, 0),
                new Point(center.X + width / 2, center.Y - height / 2, 0),
                new Point(center.X + width / 2, center.Y + height / 2, 0),
                new Point(center.X - width / 2, center.Y + height / 2, 0)
            }, color, fill);
        }

        private static void RenderPolygon(Point[] vertices, System.Drawing.Color color, bool fill)
        {
            glBegin(fill ? GL_TRIANGLE_FAN : GL_LINE_LOOP);

            glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            foreach (var vertex in vertices)
            {
                glVertex3d(vertex.X, vertex.Y, vertex.Z);
            }

            glEnd();
        }
    }
}
