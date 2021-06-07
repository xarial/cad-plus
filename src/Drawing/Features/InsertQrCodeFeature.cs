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
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Drawing.Features
{
    public interface IInsertQrCodeFeature : IDisposable
    {
        void Insert(IXDrawing drw);
    }

    public class InsertQrCodeFeature : IInsertQrCodeFeature
    {
        private readonly IXPropertyPage<QrCodeData> m_InsertQrCodePage;
        private readonly QrDataProvider m_QrDataProvider;
        private readonly QrCodePictureManager m_QrCodeManager;

        private readonly QrCodeData m_CurInsertQrCodePageData;
        private QrCodePreviewer m_CurPreviewer;

        private IXDrawing m_CurDrawing;

        private readonly IXApplication m_App;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        public InsertQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger) 
        {
            m_App = ext.Application;
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_InsertQrCodePage = ext.CreatePage<QrCodeData>();

            m_CurInsertQrCodePageData = new QrCodeData();

            m_QrDataProvider = new QrDataProvider(m_App);
            m_QrCodeManager = new QrCodePictureManager(m_App, m_QrDataProvider);

            m_InsertQrCodePage.DataChanged += OnPageDataChanged;
            m_InsertQrCodePage.Closed += OnInserQrCodePageClosed;
            m_InsertQrCodePage.Closing += OnInsertQrCodePageClosing;
        }

        private void OnInsertQrCodePageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                try
                {
                    if (string.IsNullOrEmpty(m_QrDataProvider.GetData(m_CurDrawing, m_CurInsertQrCodePageData.Source)))
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

        private void OnInserQrCodePageClosed(PageCloseReasons_e reason)
        {
            m_CurPreviewer.Dispose();
            m_CurPreviewer = null;

            if (reason == PageCloseReasons_e.Okay)
            {
                try
                {
                    var pict = m_QrCodeManager.Insert(m_CurDrawing, m_CurInsertQrCodePageData.Location, m_CurInsertQrCodePageData.Source);

                    var data = new QrCodeInfo();
                    data.Fill(m_CurInsertQrCodePageData, pict);

                    var handler = m_App.Documents.GetHandler<QrCodeDrawingHandler>(m_CurDrawing);

                    handler.QrCodes.Add(data);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    m_MsgSvc.ShowError(ex);
                }
            }
        }

        public void Insert(IXDrawing drw)
        {
            m_CurDrawing = drw;
            m_CurPreviewer = new QrCodePreviewer(m_CurDrawing, m_QrCodeManager);
            m_InsertQrCodePage.Show(m_CurInsertQrCodePageData);
            UpdatePreview();
        }

        private void OnPageDataChanged()
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            m_CurPreviewer.Preview(
                m_CurInsertQrCodePageData.Location.Dock,
                m_CurInsertQrCodePageData.Location.Size,
                m_CurInsertQrCodePageData.Location.OffsetX,
                m_CurInsertQrCodePageData.Location.OffsetY);
        }

        public void Dispose()
        {
            m_CurPreviewer?.Dispose();
        }
    }
}
