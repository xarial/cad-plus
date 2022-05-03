//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Xarial.CadPlus.CustomToolbar.UI.Views
{
    public partial class CommandMacroView : UserControl
    {
        public CommandMacroView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty WorkingDirectoryProperty =
            DependencyProperty.Register(
            nameof(WorkingDirectory), typeof(string),
            typeof(CommandMacroView));

        public string WorkingDirectory
        {
            get { return (string)GetValue(WorkingDirectoryProperty); }
            set { SetValue(WorkingDirectoryProperty, value); }
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
    }
}
