using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.InApp.UI;
using Xarial.XCad.Documents;
using Xarial.XCad.UI.PropertyPage.Attributes;

namespace Xarial.CadPlus.Batch.InApp
{
    public class AssemblyBatchData
    {
        [ControlOptions(height: 100)]
        public List<IXComponent> Components { get; set; }
        public bool ProcessAllFiles { get; set; }

        [CustomControl(typeof(MacrosList))]
        [ControlOptions(height: 100)]
        public ObservableCollection<string> Macros { get; set; } = new ObservableCollection<string>();
    }
}
