//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Diagnostics;
using System.Windows.Controls;

namespace Xarial.CadPlus.CustomToolbar.UI.Views
{
    public partial class CommandMacroView : UserControl
    {
        public CommandMacroView()
        {
            InitializeComponent();
        }

        private void OnHelpClicked(object sender, System.Windows.RoutedEventArgs e)
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
