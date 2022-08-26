//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.Windows.Input;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonButtonCommand : IRibbonCommand
    {
        ICommand Command { get; }
    }

    public class RibbonButtonCommand : RibbonCommand, IRibbonButtonCommand
    {
        public ICommand Command { get; }

        public RibbonButtonCommand(string title, Image icon, string description, ICommand command)
            : base(title, icon, description)
        {
            Command = command;
        }
    }
}
