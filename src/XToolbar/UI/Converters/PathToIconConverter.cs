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
using Xarial.CadPlus.XToolbar.Properties;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XToolbar.UI.Converters
{
    public class PathToIconConverter : IValueConverter
    {
        private static readonly BitmapImage m_DefaultIcon;

        static PathToIconConverter()
        {
            m_DefaultIcon = Resources.macro_icon_default.ToBitmapImage();
        }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconPath = value as string;

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
                icon = m_DefaultIcon;
            }

            return icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}