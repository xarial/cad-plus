//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using System.Windows.Media;
using Xarial.CadPlus.CustomToolbar.Enums;

namespace Xarial.CadPlus.Toolbar.UI.Converters
{
    public class MacroTreeItemOpacityConverter : IValueConverter
    {
        private const double ENABLED_OPACITY = 1;
        private const double DISABLED_OPACITY = 0.5;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Triggers_e) 
            {
                if ((Triggers_e)value == Triggers_e.None) 
                {
                    return DISABLED_OPACITY;
                }
            }

            return ENABLED_OPACITY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
