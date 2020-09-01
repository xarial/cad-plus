//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.CustomToolbar.UI.Converters
{
    public class PathToIconConverter : IMultiValueConverter
    {
        private static readonly BitmapImage m_DefaultMacroIcon;
        private static readonly BitmapImage m_DefaultGroupIcon;

        static PathToIconConverter()
        {
            m_DefaultMacroIcon = Resources.macro_icon_default.ToBitmapImage();
            m_DefaultGroupIcon = Resources.group_icon_default.ToBitmapImage();
        }
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var iconPath = values[0] as string;

            BitmapImage icon = null;

            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {
                try
                {
                    icon = new BitmapImage(new Uri(iconPath));
                }
                catch
                {
                }
            }

            if (icon == null)
            {
                if (values[1] is CommandMacroVM) 
                {
                    icon = m_DefaultMacroIcon;
                }
                else if (values[1] is CommandGroupVM)
                {
                    icon = m_DefaultGroupIcon;
                }
            }

            return icon;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}