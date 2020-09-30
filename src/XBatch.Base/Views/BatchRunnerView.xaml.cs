//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Globalization;
using System.Windows.Controls;
using Xarial.XToolkit.Wpf.Controls;

namespace Xarial.CadPlus.XBatch.Base.Views
{
    public partial class BatchRunnerView : UserControl
    {
        public BatchRunnerView()
        {
            InitializeComponent();
            this.DataContextChanged += BatchRunnerView_DataContextChanged;
        }

        private void BatchRunnerView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var z = new EnumValueToHeaderConverter();
            z.Convert(null, null, null, CultureInfo.InvariantCulture);
        }
    }
}
