using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xarial.CadPlus.XBatch.Base.Controls
{
    public partial class MacroDataControl : UserControl
    {
        public MacroDataControl()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty ShowFullPathProperty =
			DependencyProperty.Register(
			nameof(ShowFullPath), typeof(bool),
			typeof(MacroDataControl),
			new PropertyMetadata(true));

		public bool ShowFullPath
		{
			get { return (bool)GetValue(ShowFullPathProperty); }
			set { SetValue(ShowFullPathProperty, value); }
		}
	}
}
