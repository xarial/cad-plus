using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonDropDownButton 
    {
        object Value { get; set; }
        IEnumerable<object> ItemsSource { get; }
    }

    public class RibbonDropDownButton : IRibbonDropDownButton
    {
        public object Value 
        {
            get => m_ValueGetter.Invoke();
            set => m_ValueSetter.Invoke(value);
        }

        public IEnumerable<object> ItemsSource { get; }

        private readonly Func<object> m_ValueGetter;
        private readonly Action<object> m_ValueSetter;

        public RibbonDropDownButton(Func<object> valueGetter, Action<object> valueSetter,
            IEnumerable<object> itemsSource) 
        {
            m_ValueGetter = valueGetter;
            m_ValueSetter = valueSetter;

            ItemsSource = itemsSource;
        }
    }
}
