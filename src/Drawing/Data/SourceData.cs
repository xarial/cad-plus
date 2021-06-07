//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.Linq;
using Xarial.CadPlus.Drawing.Properties;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.CadPlus.Drawing.Data
{
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
}
