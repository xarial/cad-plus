//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Drawing.Data;
using Xarial.CadPlus.Drawing.Properties;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.CadPlus.Drawing
{
    [IconEx(typeof(Resources), nameof(Resources.qrcode_vector), nameof(Resources.qrcode_icon))]
    [Title("Insert QR Code")]
    [Help("https://cadplus.xarial.com/drawing/qr-code/")]
    public class InsertQrCodeData
    {
        public SourceData Source { get; set; }

        public LocationData Location { get; set; }

        public InsertQrCodeData() 
        {
            Source = new SourceData();
            Location = new LocationData();
        }
    }

    public class CustomPropertyNameDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            source.Visible = (Source_e)dependencies.First()?.GetValue() == Source_e.CustomProperty;
        }
    }

    public class PdmWeb2ServerDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            source.Visible = (Source_e)dependencies.First()?.GetValue() == Source_e.PdmWeb2Url;
        }
    }

    public class CustomValueDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            source.Visible = (Source_e)dependencies.First()?.GetValue() == Source_e.Custom;
        }
    }

    public class SourceData 
    {
        [IconEx(typeof(Resources), nameof(Resources.source_vector), nameof(Resources.source_icon))]
        [ControlTag(nameof(Source))]
        public Source_e Source { get; set; }

        [DependentOn(typeof(CustomPropertyNameDependencyHandler), nameof(Source))]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        [Description("Custom property name")]
        public string CustomPropertyName { get; set; }

        [DependentOn(typeof(PdmWeb2ServerDependencyHandler), nameof(Source))]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        [Description("PDM Web2 Server")]
        public string PdmWeb2Server { get; set; }

        [DependentOn(typeof(CustomValueDependencyHandler), nameof(Source))]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        [Description("Custom value")]
        public string CustomValue { get; set; }

        [Title("Refereced Document")]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public bool ReferencedDocument { get; set; }
    }

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
