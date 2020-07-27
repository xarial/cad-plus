using System;
using System.Globalization;
using System.Windows.Data;

namespace Xarial.CadPlus.XToolbar.UI.Converters
{
    public class SelectedCommandConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0] == values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}