//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.QrCode.Properties;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    [IconEx(typeof(Resources), nameof(Resources.qrcode_vector), nameof(Resources.qrcode_icon))]
    [Title("Insert QR Code")]
    [Help("https://cadplus.xarial.com/drawing/qr-code/")]
    public class QrCodeData
    {
        public SourceData Source { get; set; }

        public LocationData Location { get; set; }

        public QrCodeData()
        {
            Source = new SourceData();
            Location = new LocationData();
        }
    }
}
