//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Drawing.QrCode.Features
{
    public interface IEditQrCodeFeature : IDisposable
    {
        void UpdateInPlace(ISwObject pict, ISwDrawing drw);
        void Reload(ISwObject pict, ISwDrawing drw);
        void Edit(ISwObject pict, ISwDrawing drw);
    }

    public class EditQrCodeFeature : InsertQrCodeFeature, IEditQrCodeFeature
    {
        private ISwObject m_CurPict;
        private QrCodeInfo m_CurInfo;

        private Tuple<int, double, int, double> m_CurPictTrans;

        public EditQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger, IDocumentMetadataAccessLayerProvider docMalProvider)
            : base(ext, msgSvc, logger, docMalProvider)
        {
        }

        protected override void OnInsertQrCode()
        {
            var pict = m_QrCodeManager.Reload(m_CurPict,
                m_CurInsertQrCodePageData.Location,
                m_CurInsertQrCodePageData.Source,
                m_CurDrawing);

            m_CurInfo.Fill(m_CurInsertQrCodePageData, pict);
        }

        protected override void OnCancelInsertQrCode()
        {
            base.OnCancelInsertQrCode();
            HidePicture(m_CurPict, false);
        }

        public void Edit(ISwObject pict, ISwDrawing drw)
        {
            m_CurPict = pict;
            m_CurInfo = FindInfo(pict, drw);

            m_CurInsertQrCodePageData = m_CurInfo.ToData();

            HidePicture(m_CurPict, true);

            Insert(drw);
        }

        public void Reload(ISwObject pict, ISwDrawing drw)
        {
            var info = FindInfo(pict, drw);

            var data = info.ToData();

            info.Picture = m_QrCodeManager.Reload(pict, data.Location, data.Source, drw);
        }

        public void UpdateInPlace(ISwObject pict, ISwDrawing drw)
        {
            var data = FindInfo(pict, drw);

            data.Picture = m_QrCodeManager.UpdateInPlace(pict, data.ToData().Source, drw);
        }

        private QrCodeInfo FindInfo(ISwObject pict, ISwDrawing drw)
        {
            var handler = m_App.Documents.GetHandler<QrCodeDrawingHandler>(drw);
            var qrCode = handler.QrCodes.FirstOrDefault(d => d.Picture.Equals(pict));

            if (qrCode == null)
            {
                throw new UserException("This picture does not contain QR code data");
            }

            return qrCode;
        }

        private void HidePicture(ISwObject pict, bool hide)
        {
            var skPict = (ISketchPicture)pict.Dispatch;

            if (hide)
            {
                int style = -1;
                double trans = -1;
                int matchColor = -1;
                double matchTol = -1;

                skPict.GetTransparency(ref style, ref trans, ref matchColor, ref matchTol);
                m_CurPictTrans = new Tuple<int, double, int, double>(style, trans, matchColor, matchTol);

                const int TRANSPARENT = 1;
                const int COLOR_IGNORE = 0;
                const int COLOR_EXACT_MATCH = 0;
                skPict.SetTransparency((int)swSketchPictureTransparencyStyle_e.swSketchPictureTransparencyFullImage,
                    TRANSPARENT, COLOR_IGNORE, COLOR_EXACT_MATCH);
            }
            else
            {
                skPict.SetTransparency(m_CurPictTrans.Item1, m_CurPictTrans.Item2, m_CurPictTrans.Item3, m_CurPictTrans.Item4);
            }
        }
    }
}
