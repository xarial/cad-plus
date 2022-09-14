//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using Xarial.CadPlus.Drawing.QrCode.Properties;
using Xarial.CadPlus.Drawing.QrCode.Services;
using Xarial.CadPlus.Drawing.QrCode.UI;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    public class SourceData
    {
        [IconEx(typeof(Resources), nameof(Resources.source_vector), nameof(Resources.source_icon))]
        [Description("Expression for the QR code data")]
        [CustomControl(typeof(QrCodeExpressionBoxControl))]
        [ControlOptions(height: 15)]
        public string Expression { get; set; }

        public SourceData() 
        {
            Expression = $"{{{QrCodeDataSourceExpressionSolver.VAR_FILE_PATH} [{FilePathSource_e.FullPath}] [{false}]}}";
        }
    }
}
