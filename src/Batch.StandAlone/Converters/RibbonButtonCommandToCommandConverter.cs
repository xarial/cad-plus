using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.XToolkit.Wpf;

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class RibbonButtonCommandToCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RibbonButtonCommand) 
            {
                return new RelayCommand((value as RibbonButtonCommand).Handler,
                    (value as RibbonButtonCommand).CanExecuteHandler);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
