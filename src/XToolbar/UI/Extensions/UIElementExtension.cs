//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xarial.CadPlus.CustomToolbar.UI.Extensions
{
    public static class UIElementExtension
    {
        public static readonly DependencyProperty SelectListItemOnClickProperty = DependencyProperty.RegisterAttached(
            "SelectListItemOnClick",
            typeof(bool),
            typeof(UIElementExtension),
            new FrameworkPropertyMetadata(false, OnSelectListItemOnClickPropertyChanged));

        public static void SetSelectListItemOnClick(UIElement element, Boolean value)
        {
            element.SetValue(SelectListItemOnClickProperty, value);
        }

        public static bool GetSelectListItemOnClick(UIElement element)
        {
            return (bool)element.GetValue(SelectListItemOnClickProperty);
        }

        private static void OnSelectListItemOnClickPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selectListItemOnClick = (bool)e.NewValue;
            var elem = d as UIElement;

            if (selectListItemOnClick)
            {
                elem.PreviewMouseUp += OnToolbarPreviewMouseDown;
            }
            else
            {
                elem.PreviewMouseUp -= OnToolbarPreviewMouseDown;
            }
        }

        private static void OnToolbarPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var elem = sender as UIElement;
            var item = GetAncestorByType<ListBoxItem>(elem);

            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        public static T GetAncestorByType<T>(DependencyObject element)
            where T : UIElement
        {
            if (element == null)
            {
                return null;
            }

            if (element is T)
            {
                return element as T;
            }
            else
            {
                return GetAncestorByType<T>(VisualTreeHelper.GetParent(element));
            }
        }
    }
}
