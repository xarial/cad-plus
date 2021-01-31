using System;
using System.Drawing;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonToggleCommand : IRibbonCommand
    {
        bool Value { get; set; }
    }

    public class RibbonToggleCommand : RibbonCommand, IRibbonToggleCommand
    {
        public bool Value 
        {
            get => m_Getter.Invoke();
            set => m_Setter.Invoke(value);
        }

        private readonly Func<bool> m_Getter;
        private readonly Action<bool> m_Setter;

        public RibbonToggleCommand(string title, Image icon,
            Func<bool> getter, Action<bool> setter) : base(title, icon)
        {
            m_Getter = getter;
            m_Setter = setter;
        }
    }
}
