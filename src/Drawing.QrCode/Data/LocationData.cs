//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.CadPlus.Drawing.QrCode.Properties;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.CadPlus.Drawing.QrCode.Data
{
    public class LocationData
    {
        [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 100, 0.01, false, 0.02, 0.001)]
        [StandardControlIcon(BitmapLabelType_e.Width)]
        [IconEx(typeof(Resources), nameof(Resources.size_vector), nameof(Resources.size_icon))]
        public double Size { get; set; }

        [IconEx(typeof(Resources), nameof(Resources.dock_vector), nameof(Resources.dock_icon))]
        public Dock_e Dock { get; set; }

        [NumberBoxOptions(NumberBoxUnitType_e.Length, -1000, 1000, 0.01, false, 0.02, 0.001)]
        [IconEx(typeof(Resources), nameof(Resources.offsetx_vector), nameof(Resources.offsetx_icon))]
        public double OffsetX { get; set; }

        [NumberBoxOptions(NumberBoxUnitType_e.Length, -1000, 1000, 0.01, false, 0.02, 0.001)]
        [IconEx(typeof(Resources), nameof(Resources.offsety_vector), nameof(Resources.offsety_icon))]
        public double OffsetY { get; set; }

        public LocationData()
        {
            Dock = Dock_e.BottomRight;
            Size = 0.1;
        }
    }
}
