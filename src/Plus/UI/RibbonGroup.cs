//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonGroup
    {
        string Name { get; }
        string Title { get; }
        IList<IRibbonCommand> Commands { get; }
    }

    public class RibbonGroup : IRibbonGroup
    {
        public string Name { get; }
        public string Title { get; }
        public IList<IRibbonCommand> Commands { get; }

        public RibbonGroup(string name, string title, params IRibbonCommand[] commands) 
        {
            Name = name;
            Title = title;
            Commands = new List<IRibbonCommand>(commands ?? new IRibbonCommand[0]);
        }
    }
}
