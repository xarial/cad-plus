//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;

namespace Xarial.CadPlus.CustomToolbar.UI.Views
{
    public partial class CommandManagerView : UserControl
    {
        public class MacroDropArgs 
        {
            public string[] FilePaths { get; }
            public ICommandVM TargetCommand { get; }

            internal MacroDropArgs(string[] filePaths, ICommandVM targetCmd) 
            {
                FilePaths = filePaths;
                TargetCommand = targetCmd;
            }
        }

        public CommandManagerView()
        {
            InitializeComponent();

            var dropBinding = new Binding(nameof(CommandManagerVM.MacroDropCommand));
            this.SetBinding(CommandManagerView.FileDropCommandProperty, dropBinding);
        }

        public static readonly DependencyProperty FileDropCommandProperty =
            DependencyProperty.Register(
            nameof(FileDropCommand), typeof(ICommand),
            typeof(CommandManagerView));

        public ICommand FileDropCommand
        {
            get { return (ICommand)GetValue(FileDropCommandProperty); }
            set { SetValue(FileDropCommandProperty, value); }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (TryGetFileDropArgument(sender as TreeView, e, out MacroDropArgs args) 
                && FileDropCommand?.CanExecute(args) == true)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (TryGetFileDropArgument(sender as TreeView, e, out MacroDropArgs args)
                && FileDropCommand?.CanExecute(args) == true)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (TryGetFileDropArgument(sender as TreeView, e, out MacroDropArgs args)) 
            {
                FileDropCommand?.Execute(args);
                (sender as FrameworkElement).Focus();
            }
        }

        private bool TryGetFileDropArgument(TreeView sender, DragEventArgs e, out MacroDropArgs args) 
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];

                var cmd = GetCommandAtLocation(sender as TreeView, e.GetPosition(sender as TreeView));

                args = new MacroDropArgs(files, cmd);
                return true;
            }
            else 
            {
                args = null;
                return false;
            }
        }

        private ICommandVM GetCommandAtLocation(TreeView treeView, Point location)
        {
            ICommandVM foundCmd = null;

            var hitTestResults = VisualTreeHelper.HitTest(treeView, location);

            if (hitTestResults.VisualHit is FrameworkElement)
            {
                var dataObject = (hitTestResults.VisualHit as
                    FrameworkElement).DataContext;

                if (dataObject is ICommandVM)
                {
                    foundCmd = (ICommandVM)dataObject;
                }
            }

            return foundCmd;
        }
    }
}
