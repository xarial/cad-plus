﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xarial.CadPlus.Plus.Shared.Controls;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Shared.Converters
{
    public class CadObjectContextMenuVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IXDocument || (value is ICustomObject && ((ICustomObject)value).CanOpenInExplorer))
            {
                return Visibility.Visible;
            }
            else 
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}