using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xarial.CadPlus.XBatch.MDI.Converters
{
    public class DocumentTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var name = values[0] as string;
            var isDirty = true;

            if (values[1] is bool) 
            {
                isDirty = (bool)values[1];
            }

            if (isDirty)
            {
                return name + "*";
            }
            else 
            {
                return name;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
