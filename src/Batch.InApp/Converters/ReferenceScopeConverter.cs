using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.InApp.Converters
{
    public class ReferenceScopeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool && values[1] is IXDocument3D[] && values[2] is IXDocument3D[])
            {
                var topLevelOnly = (bool)values[0];

                if (topLevelOnly)
                {
                    return values[1];
                }
                else 
                {
                    return values[2];
                }
            }
            else 
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
