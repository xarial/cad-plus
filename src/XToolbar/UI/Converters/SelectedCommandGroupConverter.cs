using System;
using System.Globalization;
using System.Windows.Data;
using Xarial.CadPlus.XToolbar.UI.ViewModels;

namespace Xarial.CadPlus.XToolbar.UI.Converters
{
    public class SelectedCommandGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CommandGroupVM))
            {
                return null;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}