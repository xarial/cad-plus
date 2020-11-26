using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.InApp.UI;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.CadPlus.Batch.InApp.Properties;

namespace Xarial.CadPlus.Batch.InApp
{
    public class NotProcessAllFilesDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var isChecked = dependencies.First()?.GetValue();

            if (isChecked is bool) 
            {
                source.Enabled = !(bool)isChecked; 
            }
        }
    }

    public class AssemblyBatchData
    {
        [ControlOptions(height: 100)]
        [DependentOn(typeof(NotProcessAllFilesDependencyHandler), nameof(ProcessAllFiles))]
        public List<IXComponent> Components { get; set; }

        [Title("Process All Files")]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        [ControlTag(nameof(ProcessAllFiles))]
        public bool ProcessAllFiles { get; set; } = true;

        [Description("Open each document in its own window (activate)")]
        [Title("Activate Documents")]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public bool ActivateDocuments { get; set; } = true;

        [CustomControl(typeof(MacrosList))]
        [ControlOptions(height: 100)]
        [Icon(typeof(Resources), nameof(Resources.macros_icon))]
        public ObservableCollection<string> Macros { get; set; } = new ObservableCollection<string>();
    }
}
