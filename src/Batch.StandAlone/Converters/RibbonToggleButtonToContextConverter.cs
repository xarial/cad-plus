using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;

namespace Xarial.CadPlus.Batch.StandAlone.Converters
{
    public class RibbonToggleButtonToContextConverter : IValueConverter
    {
        public class RibbonToggleButtonDataContext
        {
            public bool Value 
            {
                get => m_Getter.Invoke();
                set => m_Setter.Invoke(value);
            }

            public Func<bool> m_Getter;
            public Action<bool> m_Setter;

            internal RibbonToggleButtonDataContext(Func<bool> getter, Action<bool> setter) 
            {
                m_Getter = getter;
                m_Setter = setter;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ToggleButtonCommand) 
            {
                return new RibbonToggleButtonDataContext(
                    (value as ToggleButtonCommand).Getter,
                    (value as ToggleButtonCommand).Setter);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
