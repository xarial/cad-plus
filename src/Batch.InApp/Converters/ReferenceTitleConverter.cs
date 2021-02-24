using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.InApp.Converters
{
    public class ReferenceTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IXDocument)
            {
                var doc = value as IXDocument;
                
                var title = doc.Title;
                
                if (string.IsNullOrEmpty(title))
                {
                    title = Path.GetFileName(doc.Path);
                }

                return title;
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
