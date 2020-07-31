//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace Xarial.CadPlus.CustomToolbar.UI.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class ObjectIsNotNullUniversalConverter : IValueConverter
    {
        public object TrueValue { get; set; } = true;
        public object FalseValue { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is null))
            {
                return TrueValue;
            }
            else
            {
                return FalseValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
