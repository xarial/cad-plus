//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        public bool Value 
        {
            get => m_Getter.Invoke();
            set
            {
                m_Setter.Invoke(value);
                NotifyChanged(nameof(Value));
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

        public override void Update()
        {
            base.Update();

            this.NotifyChanged(nameof(Value));
        }
    }
}
