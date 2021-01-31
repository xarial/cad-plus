using System;
using System.Drawing;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonButtonCommand : IRibbonCommand
    {
        Action Handler { get; }
        Func<bool> CanExecuteHandler { get; }
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
}
