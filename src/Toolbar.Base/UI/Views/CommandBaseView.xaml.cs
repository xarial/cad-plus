//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;

namespace Xarial.CadPlus.CustomToolbar.UI.Views
{
    public partial class CommandBaseView : UserControl
    {
        public CommandBaseView()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty WorkingDirectoryProperty =
			DependencyProperty.Register(
			nameof(WorkingDirectory), typeof(string),
			typeof(CommandBaseView));

		public string WorkingDirectory
		{
			get { return (string)GetValue(WorkingDirectoryProperty); }
			set { SetValue(WorkingDirectoryProperty, value); }
		}
	}
}
