using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonNumericSwitchCommand : IRibbonSwitchCommand 
    {
        double NumericValue { get; set; }
    }

    public class RibbonNumericSwitchCommand : RibbonSwitchCommand, IRibbonNumericSwitchCommand
    {
        public double NumericValue
        {
            get => m_NumericGetter.Invoke();
            set => m_NumericSetter.Invoke(value);
        }

        private readonly Func<double> m_NumericGetter;
        private readonly Action<double> m_NumericSetter;

        public RibbonNumericSwitchCommand(string title, Image icon,
            Func<bool> getter, Action<bool> setter,
            Func<double> numGetter, Action<double> numSetter)
            : base(title, icon, getter, setter)
        {
            m_NumericGetter = numGetter;
            m_NumericSetter = numSetter;
        }
    }
}
