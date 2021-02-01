using System;
using System.ComponentModel;
using System.Drawing;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonToggleCommand : IRibbonCommand
    {
        bool Value { get; set; }
    }

    public class RibbonToggleCommand : RibbonCommand, IRibbonToggleCommand, INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        public bool Value 
        {
            get => m_Getter.Invoke();
            set
            {
                m_Setter.Invoke(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        private readonly Func<bool> m_Getter;
        private readonly Action<bool> m_Setter;

        public RibbonToggleCommand(string title, Image icon, string description,
            Func<bool> getter, Action<bool> setter) : base(title, icon, description)
        {
            m_Getter = getter;
            m_Setter = setter;
        }
    }
}
