//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Drawing.Data;
using Xarial.CadPlus.Drawing.Services;
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

namespace Xarial.CadPlus.Drawing.Features
{
    public interface IEditQrCodeFeature : IDisposable
    {
        void UpdateInPlace(ISwObject pict, ISwDrawing drw);
        void Reload(ISwObject pict, ISwDrawing drw);
        void Edit(ISwObject pict, ISwDrawing drw);
    }

    public class EditQrCodeFeature : IEditQrCodeFeature
    {
        private readonly IXPropertyPage<QrCodeData> m_EditQrCodePage;
        private readonly QrDataProvider m_QrDataProvider;
        private readonly QrCodePictureManager m_QrCodeManager;

        private QrCodeData m_CurEditQrCodePageData;

        private IXDrawing m_CurDrawing;
        private ISwObject m_CurPict;
        private QrCodeInfo m_CurrentData;

        private readonly IXApplication m_App;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        public EditQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger) 
        {
            m_App = ext.Application;
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_EditQrCodePage = ext.CreatePage<QrCodeData>();

            m_CurEditQrCodePageData = new QrCodeData();

            m_QrDataProvider = new QrDataProvider(m_App);
            m_QrCodeManager = new QrCodePictureManager(m_App, m_QrDataProvider);

            m_EditQrCodePage.Closed += OnEditQrCodePageClosed;
            m_EditQrCodePage.Closing += OnEditQrCodePageClosing;
        }

        private void OnEditQrCodePageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                try
                {
                    if (string.IsNullOrEmpty(m_QrDataProvider.GetData(m_CurDrawing, m_CurEditQrCodePageData.Source)))
                    {
                        throw new UserException("Data for QR code is empty");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    arg.Cancel = true;
                    arg.ErrorMessage = ex.ParseUserError(out _);
                }
            }
        }

        private void OnEditQrCodePageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                try
                {
                    //TODO: delete 
                    var pict = m_QrCodeManager.Insert(m_CurDrawing, m_CurEditQrCodePageData.Location, m_CurEditQrCodePageData.Source);
                    m_CurrentData.Fill(m_CurEditQrCodePageData, pict);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    m_MsgSvc.ShowError(ex);
                }
            }
            else if(reason == PageCloseReasons_e.Cancel)
            {
                HidePicture(m_CurPict, false);
            }
        }

        public void Edit(ISwObject pict, ISwDrawing drw)
        {
            m_CurPict = pict;
            m_CurDrawing = drw;
            m_CurrentData = FindInfo(pict, drw);

            m_CurEditQrCodePageData = m_CurrentData.ToData();

            HidePicture(m_CurPict, true);

            m_EditQrCodePage.Show(m_CurEditQrCodePageData);
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

        private Tuple<int, double, int, double> m_CurPictTrans;

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

        public void Dispose()
        {
        }
    }
}
