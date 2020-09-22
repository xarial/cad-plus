//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
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
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Controls
{
    public class PathListBox : Control
    {
        private ListBox m_ListBox;
        private Button m_AddFileButton;
        private Button m_AddFolderButton;

        static PathListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PathListBox), new FrameworkPropertyMetadata(typeof(PathListBox)));
        }

        public static readonly DependencyProperty PathsSourceProperty =
            DependencyProperty.Register(
            nameof(PathsSource), typeof(IList),
            typeof(PathListBox));

        public IList PathsSource
        {
            get { return (IList)GetValue(PathsSourceProperty); }
            set { SetValue(PathsSourceProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            m_ListBox = (ListBox)this.Template.FindName("PART_ListBox", this);
            m_AddFileButton = (Button)this.Template.FindName("PART_AddFileButton", this);
            m_AddFolderButton = (Button)this.Template.FindName("PART_AddFolderButton", this);

            m_ListBox.KeyUp += OnListBoxKeyUp;
            m_AddFileButton.Click += OnAddFileButtonClick;
            m_AddFolderButton.Click += OnAddFolderButtonClick;
        }

        private void OnAddFileButtonClick(object sender, RoutedEventArgs e)
        {
            AddFile();
        }

        private void OnAddFolderButtonClick(object sender, RoutedEventArgs e)
        {
            AddFolder();
        }

        private void AddFolder()
        {
            if (FileSystemBrowser.BrowseFolder(out string path, "Select folder to process"))
            {
                PathsSource?.Add(path);
            }
        }

        private void AddFile()
        {
            var filter = FileSystemBrowser.BuildFilterString(
                new FileFilter("SOLIDWORKS Files", "*.sldprt", "*.sldasm", "*.slddrw"),
                FileFilter.AllFiles);

            if (FileSystemBrowser.BrowseFileOpen(out string path, "Select file to process", filter))
            {
                PathsSource?.Add(path);
            }
        }

        private void OnListBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) 
            {
                var itemsToDelete = new object[m_ListBox.SelectedItems.Count];
                m_ListBox.SelectedItems.CopyTo(itemsToDelete, 0);

                foreach (var selItem in itemsToDelete) 
                {
                    PathsSource.Remove(selItem);
                }
            }
        }
    }
}
