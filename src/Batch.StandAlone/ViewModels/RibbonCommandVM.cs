using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public abstract class RibbonCommand 
    {
        public Image Icon { get; }
        public string Title { get; }

        public RibbonCommand(string title, Image icon) 
        {
            Title = title;
            Icon = icon;
        }
    }

    public class RibbonButtonCommand : RibbonCommand
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

    public class ToggleButtonCommand : RibbonCommand
    {
        public Func<bool> Getter { get; }
        public Action<bool> Setter { get; }

        public ToggleButtonCommand(string title, Image icon,
            Func<bool> getter, Action<bool> setter) : base(title, icon)
        {
            Getter = getter;
            Setter = setter;
        }
    }
}
