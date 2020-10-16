using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xarial.CadPlus.XBatch.Base.Converters
{
    public class LogOutputToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var lines = values.First() as IEnumerable<string>;

            var logText = new StringBuilder();

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    logText.AppendLine(line?.ToString());
                }
            }

            return logText.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
