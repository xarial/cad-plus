//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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

        public RibbonButtonCommand(string title, Image icon, string description, Action handler, Func<bool> canExecute)
            : base(title, icon, description)
        {
            Handler = handler;
            CanExecuteHandler = canExecute;
        }
    }
}
