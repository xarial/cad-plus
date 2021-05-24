//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        void Update(ISwObject pict, ISwDrawing drw);
        void Edit(ISwObject pict, ISwDrawing drw);
    }

    public class EditQrCodeFeature : IEditQrCodeFeature
    {
        private readonly IXPropertyPage<EditQrCodeData> m_EditQrCodePage;
        private readonly QrDataProvider m_QrDataProvider;
        private readonly QrCodeManager m_QrCodeManager;

        private readonly EditQrCodeData m_CurEditQrCodePageData;

        private IXDrawing m_CurDrawing;
        private ISwObject m_CurPict;
        private QrCodeData m_CurrentData;

        private readonly IXApplication m_App;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        public EditQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger) 
        {
            m_App = ext.Application;
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_EditQrCodePage = ext.CreatePage<EditQrCodeData>();

            m_CurEditQrCodePageData = new EditQrCodeData();

            m_QrDataProvider = new QrDataProvider(m_App);
            m_QrCodeManager = new QrCodeManager(m_App, m_QrDataProvider);

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
                    var pict = m_QrCodeManager.Update(m_CurPict, m_CurEditQrCodePageData.Source, m_CurDrawing);
                    m_CurrentData.Fill(m_CurEditQrCodePageData.Source, pict);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    m_MsgSvc.ShowError(ex);
                }
            }
        }

        public void Edit(ISwObject pict, ISwDrawing drw)
        {
            m_CurPict = pict;
            m_CurDrawing = drw;
            m_CurrentData = FindData(pict, drw);

            m_CurEditQrCodePageData.Source = m_CurrentData.ToSourceData();

            m_EditQrCodePage.Show(m_CurEditQrCodePageData);
        }

        public void Update(ISwObject pict, ISwDrawing drw)
        {
            var data = FindData(pict, drw);

            data.Picture = m_QrCodeManager.Update(pict, data.ToSourceData(), drw);
        }

        private QrCodeData FindData(ISwObject pict, ISwDrawing drw) 
        {
            var handler = m_App.Documents.GetHandler<QrCodeDrawingHandler>(drw);
            var qrCode = handler.QrCodes.FirstOrDefault(d => d.Picture.Equals(pict));

            if (qrCode == null)
            {
                throw new UserException("This picture does not contain QR code data");
            }

            return qrCode;
        }

        public void Dispose()
        {
        }
    }
}
