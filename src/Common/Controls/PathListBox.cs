//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using WK.Libraries.BetterFolderBrowserNS;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Controls
{
    public class WatermarkVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && (int)value > 0)
            {
                return Visibility.Collapsed;
            }
            else 
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

        public ICommand AddFoldersCommand { get; }
        public ICommand AddFilesCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        public static readonly DependencyProperty PathsSourceProperty =
            DependencyProperty.Register(
            nameof(PathsSource), typeof(IList),
            typeof(PathListBox));

        public IList PathsSource
        {
            get { return (IList)GetValue(PathsSourceProperty); }
            set { SetValue(PathsSourceProperty, value); }
        }

        public static readonly DependencyProperty ShowAddFileButtonProperty =
            DependencyProperty.Register(
            nameof(ShowAddFileButton), typeof(bool),
            typeof(PathListBox), new PropertyMetadata(true));

        public bool ShowAddFileButton
        {
            get { return (bool)GetValue(ShowAddFileButtonProperty); }
            set { SetValue(ShowAddFileButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowAddFolderButtonProperty =
            DependencyProperty.Register(
            nameof(ShowAddFolderButton), typeof(bool),
            typeof(PathListBox), new PropertyMetadata(true));

        public bool ShowAddFolderButton
        {
            get { return (bool)GetValue(ShowAddFolderButtonProperty); }
            set { SetValue(ShowAddFolderButtonProperty, value); }
        }

        public static readonly DependencyProperty AllowDropFilesProperty =
            DependencyProperty.Register(
            nameof(AllowDropFiles), typeof(bool),
            typeof(PathListBox), new PropertyMetadata(true));

        public bool AllowDropFiles
        {
            get { return (bool)GetValue(AllowDropFilesProperty); }
            set { SetValue(AllowDropFilesProperty, value); }
        }

        public static readonly DependencyProperty AllowDropFoldersProperty =
            DependencyProperty.Register(
            nameof(AllowDropFolders), typeof(bool),
            typeof(PathListBox), new PropertyMetadata(true));

        public bool AllowDropFolders
        {
            get { return (bool)GetValue(AllowDropFoldersProperty); }
            set { SetValue(AllowDropFoldersProperty, value); }
        }

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register(
            nameof(Filters), typeof(IEnumerable),
            typeof(PathListBox));

        public IEnumerable Filters
        {
            get { return (IEnumerable)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
            nameof(ItemTemplate), typeof(DataTemplate),
            typeof(PathListBox));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemToPathConverterProperty =
            DependencyProperty.Register(
            nameof(ItemToPathConverter), typeof(Func<object, string>),
            typeof(PathListBox));

        public Func<object, string> ItemToPathConverter
        {
            get { return (Func<object, string>)GetValue(ItemToPathConverterProperty); }
            set { SetValue(ItemToPathConverterProperty, value); }
        }

        public static readonly DependencyProperty PathToItemConverterProperty =
            DependencyProperty.Register(
            nameof(PathToItemConverter), typeof(Func<string, object>),
            typeof(PathListBox));

        public Func<string, object> PathToItemConverter
        {
            get { return (Func<string, object>)GetValue(PathToItemConverterProperty); }
            set { SetValue(PathToItemConverterProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
            nameof(Watermark), typeof(string),
            typeof(PathListBox));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public PathListBox() 
        {
            AddFilesCommand = new RelayCommand(AddFiles);
            AddFoldersCommand = new RelayCommand(AddFolders);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected);
        }

        public override void OnApplyTemplate()
        {
            m_ListBox = (ListBox)this.Template.FindName("PART_ListBox", this);
            m_AddFileButton = (Button)this.Template.FindName("PART_AddFilesButton", this);
            m_AddFolderButton = (Button)this.Template.FindName("PART_AddFoldersButton", this);
            
            m_ListBox.AllowDrop = true;

            m_ListBox.DragEnter += OnDragEnter;
            m_ListBox.DragOver += OnDragOver;
            m_ListBox.Drop += OnDrop;

            m_ListBox.KeyUp += OnListBoxKeyUp;
            m_AddFileButton.Click += OnAddFilesButtonClick;
            m_AddFolderButton.Click += OnAddFoldersButtonClick;
        }

        public void AddFiles()
        {
            var filter = "";

            if (Filters != null)
            {
                var filters = Filters?.Cast<FileFilter>()?.ToArray();
                filter = FileSystemBrowser.BuildFilterString(filters);
            }

            if (FileSystemBrowser.BrowseFilesOpen(out string[] paths, "Select file to process", filter))
            {
                AddPathsToSource(paths);
            }
        }

        public void AddFolders()
        {
            var dlg = new BetterFolderBrowser()
            {
                Title = "Select folder to process",
                Multiselect = true
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folders = dlg.SelectedFolders;

                AddPathsToSource(folders);
            }
        }

        public void DeleteSelected()
        {
            var itemsToDelete = new object[m_ListBox.SelectedItems.Count];
            m_ListBox.SelectedItems.CopyTo(itemsToDelete, 0);

            foreach (var selItem in itemsToDelete)
            {
                PathsSource.Remove(selItem);
            }
        }

        private void OnAddFilesButtonClick(object sender, RoutedEventArgs e)
            => AddFiles();

        private void OnAddFoldersButtonClick(object sender, RoutedEventArgs e)
            => AddFolders();

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (CanDrop(e))
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
            if (CanDrop(e))
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
            if (TryGetPaths(e, out string[] paths))
            {
                AddPathsToSource(paths);

                //NOTE: this is required so the command state can be reevaluated
                (sender as FrameworkElement).Focus();
            }
        }

        private bool CanDrop(DragEventArgs e)
        {
            if (TryGetPaths(e, out string[] paths))
            {
                return paths.All(p => 
                {
                    var isFile = File.Exists(p);
                    var isFolder = Directory.Exists(p);

                    if (isFile && AllowDropFiles)
                    {
                        return true;
                    }
                    else if (isFolder && AllowDropFolders)
                    {
                        return true;
                    }
                    else 
                    {
                        return false;
                    }
                });
            }

            return false;
        }

        private void AddPathsToSource(params string[] paths) 
        {
            if (paths != null) 
            {
                foreach (var path in paths) 
                {
                    if (!ContainsPath(path))
                    {
                        object item;

                        if (PathToItemConverter != null)
                        {
                            item = PathToItemConverter.Invoke(path);
                        }
                        else 
                        {
                            item = path;
                        }

                        PathsSource?.Add(item);
                    }
                }
            }
        }

        private bool ContainsPath(string path) 
        {
            if (PathsSource != null)
            {
                foreach (object curItem in PathsSource)
                {
                    string curPath;

                    if (ItemToPathConverter != null)
                    {
                        curPath = ItemToPathConverter.Invoke(curItem);
                    }
                    else 
                    {
                        curPath = curItem.ToString();
                    }

                    if (string.Equals(curPath, path, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetPaths(DragEventArgs e, out string[] paths)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                paths = e.Data.GetData(DataFormats.FileDrop) as string[];
                return true;
            }
            else
            {
                paths = null;
                return false;
            }
        }

        private void OnListBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteSelected();
            }
        }
    }
}
