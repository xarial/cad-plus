//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Batch.Base.Controls
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

		private void OnHelpClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start("https://cadplus.xarial.com/macro-arguments/");
			}
			catch 
			{
			}
		}

		private void OnOpenInFileExplorer(object sender, RoutedEventArgs e)
		{
			try
			{
				var path = (this.DataContext as MacroData).FilePath;

				if (System.IO.Directory.Exists(path))
				{
					FileSystemUtils.BrowseFolderInExplorer(path);
				}
				else if (System.IO.File.Exists(path))
				{
					FileSystemUtils.BrowseFileInExplorer(path);
				}
			}
			catch
			{
			}
		}
	}
}
