//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Drawing.QrCode;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.CadPlus.Drawing.QrCode.Data;
using Xarial.CadPlus.Plus.Modules;

namespace Xarial.CadPlus.Drawing.QrCode.Features
{
    public class InsertQrCodeFeature
    {
        private readonly IXPropertyPage<QrCodeData> m_InsertQrCodePage;

        protected QrCodeData m_PageData;

        protected QrCodeElement m_CurQrCodeElement;
        private QrCodePreviewer m_CurPreviewer;

        protected readonly IXApplication m_App;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        protected readonly ExpressionSolveErrorHandlerDelegate m_ErrorHandler;
        
        public InsertQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger, ExpressionSolveErrorHandlerDelegate errHandler)
        {
            m_App = ext.Application;
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_ErrorHandler = errHandler;

            m_InsertQrCodePage = ext.CreatePage<QrCodeData>();

            m_PageData = new QrCodeData();

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
                    if (string.IsNullOrEmpty(m_PageData.Source.Expression))
                    {
                        throw new UserException("Data for QR code is empty");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    arg.Cancel = true;
                    arg.ErrorMessage = ex.ParseUserError();
                }
            }
        }

        private void OnInserQrCodePageClosed(PageCloseReasons_e reason)
        {
            try
            {
                m_CurPreviewer.Dispose();
                m_CurPreviewer = null;

                if (reason == PageCloseReasons_e.Okay)
                {
                    InsertQrCode();
                }
                else if (reason == PageCloseReasons_e.Cancel)
                {
                    CancelInsertQrCode();
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        protected virtual void InsertQrCode()
        {
            m_CurQrCodeElement.Create(m_PageData.Location.Dock,
                m_PageData.Location.Size,
                m_PageData.Location.OffsetX,
                m_PageData.Location.OffsetY,
                m_PageData.Source.Expression, m_ErrorHandler);
        }

        protected virtual void CancelInsertQrCode()
        {
        }

        public void Insert(QrCodeElement qrCodeElem)
        {
            m_CurQrCodeElement = qrCodeElem;
            m_CurPreviewer = new QrCodePreviewer(qrCodeElem.Drawing);
            m_InsertQrCodePage.Show(m_PageData);
            UpdatePreview();
        }

        private void OnPageDataChanged()
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            m_CurPreviewer.Preview(
                m_PageData.Location.Dock,
                m_PageData.Location.Size,
                m_PageData.Location.OffsetX,
                m_PageData.Location.OffsetY);
        }

        public void Dispose()
        {
            m_CurPreviewer?.Dispose();
        }
    }
}
