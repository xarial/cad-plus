using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class CommandGroupSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO: this is temp only - select from the tab, not groups by parameter (name)
            if (value is IRibbonCommandManager) 
            {
                return (value as IRibbonCommandManager).Tabs.First().Groups.First();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
