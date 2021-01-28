using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonCommand
    {
        Image Icon { get; }
        string Title { get; }
    }

    public interface IRibbonButtonCommand : IRibbonCommand
    {
        Action Handler { get; }
        Func<bool> CanExecuteHandler { get; }
    }

    public interface IRibbonToggleCommand : IRibbonCommand 
    {
        bool Value { get; set; }
    }

    public abstract class RibbonCommand : IRibbonCommand
    {
        public Image Icon { get; }
        public string Title { get; }

        public RibbonCommand(string title, Image icon)
        {
            Title = title;
            Icon = icon;
        }
    }

    public class RibbonButtonCommand : RibbonCommand, IRibbonButtonCommand
    {
        public Action Handler { get; }
        public Func<bool> CanExecuteHandler { get; }

        public RibbonButtonCommand(string title, Image icon, Action handler, Func<bool> canExecute)
            : base(title, icon)
        {
            Handler = handler;
            CanExecuteHandler = canExecute;
        }
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
