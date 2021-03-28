using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonDropDownButton : IRibbonCommand
    {
        object Value { get; set; }
        IEnumerable<object> ItemsSource { get; }
    }

    public class RibbonDropDownButton : RibbonCommand, IRibbonDropDownButton
    {
        public object Value 
        {
            get => m_ValueGetter.Invoke();
            set
            {
                m_ValueSetter.Invoke(value);
                NotifyChanged(nameof(Value));
            }
        }

        public IEnumerable<object> ItemsSource { get; }

        private readonly Func<object> m_ValueGetter;
        private readonly Action<object> m_ValueSetter;

        public RibbonDropDownButton(string title, Image icon, string description,
            Func<object> valueGetter, Action<object> valueSetter, IEnumerable<object> itemsSource) 
            : base(title, icon, description)
        {
            m_ValueGetter = valueGetter;
            m_ValueSetter = valueSetter;

            ItemsSource = itemsSource;
        }

        public override void Update()
        {
            base.Update();

            this.NotifyChanged(nameof(Value));
            this.NotifyChanged(nameof(ItemsSource));
        }
    }
}
