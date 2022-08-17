//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
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
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Plus.Shared.Controls
{
    public class CadObjectLabelControl : Control
    {
        static CadObjectLabelControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CadObjectLabelControl),
                new FrameworkPropertyMetadata(typeof(CadObjectLabelControl)));
        }

        public override void OnApplyTemplate()
        {
            var menuItem = (MenuItem)this.Template.FindName("PART_ContextMenu_OpenInExplorer", this);
            menuItem.Click += OnOpenInFileExplorer;
        }

        public static readonly DependencyProperty DescriptorProperty =
            DependencyProperty.Register(
            nameof(Descriptor), typeof(ICadDescriptor),
            typeof(CadObjectLabelControl));

        public ICadDescriptor Descriptor
        {
            get { return (ICadDescriptor)GetValue(DescriptorProperty); }
            set { SetValue(DescriptorProperty, value); }
        }
        
        public static readonly DependencyProperty ObjectProperty =
            DependencyProperty.Register(
            nameof(Object), typeof(IXObject),
            typeof(CadObjectLabelControl));

        public IXObject Object
        {
            get { return (IXObject)GetValue(ObjectProperty); }
            set { SetValue(ObjectProperty, value); }
        }

        private void OnOpenInFileExplorer(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = "";

                switch (Object)
                {
                    case IXDocument doc:
                        path = doc.Path;
                        break;
                }

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
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
