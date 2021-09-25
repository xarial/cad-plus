using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Xarial.CadPlus.Batch.Base.Converters
{
    public class IssuesToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable)
            {
                var issues = new StringBuilder();

                foreach (var issue in (IEnumerable)value) 
                {
                    if (issues.Length != 0) 
                    {
                        issues.AppendLine();
                    }

                    issues.Append(issue?.ToString());
                }

                return issues.ToString();
            }
            else 
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
