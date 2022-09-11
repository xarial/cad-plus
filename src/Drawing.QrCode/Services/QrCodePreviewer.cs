//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using static Xarial.CadPlus.Drawing.QrCode.OpenGL;

namespace Xarial.CadPlus.Drawing.QrCode.Services
{
    public class QrCodePreviewer : IDisposable
    {
        private IXDrawing m_Drw;
        private ModelView m_View;

        private Point m_CenterPoint;
        private double? m_Size;

        public QrCodePreviewer(IXDrawing drw)
        {
            m_Drw = drw;

            m_View = ((ISwDrawing)m_Drw).Model.IActiveView;
            m_View.BufferSwapNotify += OnBufferSwapNotify;
        }

        public void Preview(QrCodeDock_e dock, double size, double offsetX, double offsetY)
        {
            m_Size = size;
            QrCodeElement.CalculateLocation(m_Drw.Sheets.Active, dock, size, offsetX, offsetY, out m_CenterPoint);

            ((ISwDrawing)m_Drw).Model.GraphicsRedraw2();
        }

        private int OnBufferSwapNotify()
        {
            if (m_CenterPoint != null && m_Size.HasValue)
            {
                RenderQrCodeTemplate(m_CenterPoint, m_Size.Value);
            }

            return 0;
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

        private void RenderRectangle2D(Point center, double width, double height,
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

        private void RenderPolygon(Point[] vertices, System.Drawing.Color color, bool fill)
        {
            glBegin(fill ? GL_TRIANGLE_FAN : GL_LINE_LOOP);

            glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            foreach (var vertex in vertices)
            {
                glVertex3d(vertex.X, vertex.Y, vertex.Z);
            }

            glEnd();
        }

        public void Dispose()
        {
            m_View.BufferSwapNotify -= OnBufferSwapNotify;
            ((ISwDrawing)m_Drw).Model.GraphicsRedraw2();
        }
    }
}
