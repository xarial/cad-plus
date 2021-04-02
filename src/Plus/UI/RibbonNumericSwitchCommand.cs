//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class RibbonNumericSwitchCommandOptions
    {
        public double Minimum { get; }
        public double Maximum { get; }

        public bool HideWhenUnchecked { get; }

        public string Format { get; }

        public RibbonNumericSwitchCommandOptions(double min, double max, bool hideOnUnchecked, string format) 
        {
            Minimum = min;
            Maximum = max;
            HideWhenUnchecked = hideOnUnchecked;
            Format = format;
        }
    }

    public class RibbonNumericSwitchCommand : RibbonSwitchCommand, IRibbonNumericSwitchCommand
    {
        public double NumericValue
        {
            get => m_NumericGetter.Invoke();
            set
            {
                m_NumericSetter.Invoke(value);
                NotifyChanged(nameof(NumericValue));
            }
        }

        public RibbonNumericSwitchCommandOptions Options { get; }

        private readonly Func<double> m_NumericGetter;
        private readonly Action<double> m_NumericSetter;

        public RibbonNumericSwitchCommand(string title, Image icon,
            string description, string onText, string offText,
            Func<bool> getter, Action<bool> setter,
            Func<double> numGetter, Action<double> numSetter, RibbonNumericSwitchCommandOptions opts)
            : base(title, icon, description, onText, offText, getter, setter)
        {
            m_NumericGetter = numGetter;
            m_NumericSetter = numSetter;

            Options = opts;
        }

        public override void Update()
        {
            base.Update();

            this.NotifyChanged(nameof(NumericValue));
            this.NotifyChanged(nameof(Options));
        }
    }
}
