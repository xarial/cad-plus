using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Plus.UI;
using Xarial.XToolkit.Wpf;

namespace Xarial.CadPlus.Plus.Shared.UI
{
    public class DropDownButtonItemCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var btn = value as IRibbonDropDownButton;

            if (btn != null)
            {
                return new RelayCommand<object>(x => btn.Value = x);
            }
            else 
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
