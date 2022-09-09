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
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Drawing.QrCode.Features
{
    public class EditQrCodeFeature : InsertQrCodeFeature
    {
        public EditQrCodeFeature(IXExtension ext, IMessageService msgSvc, IXLogger logger)
            : base(ext, msgSvc, logger)
        {
        }

        public void Edit(QrCodeElement qrCodeElem)
        {
            m_PageData = QrCodeData.FromQrCodeInfo(qrCodeElem.Info);

            qrCodeElem.Hide();

            Insert(qrCodeElem);
        }

        protected override void InsertQrCode()
        {
            m_CurQrCodeElement.Edit(m_PageData.Location.Dock, m_PageData.Location.Size,
                m_PageData.Location.OffsetX, m_PageData.Location.OffsetY,
                m_PageData.Source.Expression);
        }

        protected override void CancelInsertQrCode()
        {
            base.CancelInsertQrCode();
            m_CurQrCodeElement.Show();
        }
    }
}
