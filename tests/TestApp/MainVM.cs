using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestApp.Properties;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.UI;
using Xarial.XToolkit.Wpf.Extensions;

namespace TestApp
{
    public class MainVM
    {
        public ProgressPanelVM ProgressPanel { get; }
        public RibbonVM Ribbon { get; }
        public ObjectLabelVM ObjectLabel { get; }
        public BatchJobVM JobResult { get; }

        public MainVM() 
        {
            ProgressPanel = new ProgressPanelVM();
            Ribbon = new RibbonVM();
            ObjectLabel = new ObjectLabelVM();
            JobResult = new BatchJobVM();
        }
    }
}
