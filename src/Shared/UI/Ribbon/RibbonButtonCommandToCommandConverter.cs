//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
    public class RibbonButtonCommandToCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IRibbonButtonCommand) 
            {
                return new RelayCommand((value as IRibbonButtonCommand).Handler,
                    (value as IRibbonButtonCommand).CanExecuteHandler);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
