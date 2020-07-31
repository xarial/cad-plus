//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;

namespace Xarial.CadPlus.CustomToolbar.UI.Controls
{
    public class TreeViewEx : TreeView
    {
        public TreeViewEx()
            : base()
        {
            SelectedItemChanged += OnSelectedItemChanged;
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(SelectedItemProperty, e.NewValue);
        }

        public new object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly new DependencyProperty SelectedItemProperty 
            = DependencyProperty.Register(nameof(SelectedItem), typeof(object), 
                typeof(TreeViewEx), new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            if (e.NewValue != null)
            {
                var treeViewItem = (d as TreeView).ItemContainerGenerator
                    .ContainerFromItem(e.NewValue) as TreeViewItem;

                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                }
            }
        }
    }
}
