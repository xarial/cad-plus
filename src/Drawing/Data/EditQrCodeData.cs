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
using Xarial.CadPlus.Drawing.Properties;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;

namespace Xarial.CadPlus.Drawing.Data
{
    [IconEx(typeof(Resources), nameof(Resources.qr_code_edit_vector), nameof(Resources.qr_code_edit))]
    [Title("Edit QR Code")]
    [Help("https://cadplus.xarial.com/drawing/qr-code/")]
    public class EditQrCodeData
    {
        public SourceData Source { get; set; }

        public EditQrCodeData()
        {
            Source = new SourceData();
        }
    }
}
