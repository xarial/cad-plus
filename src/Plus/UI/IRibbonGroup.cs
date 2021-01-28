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
        List<IRibbonCommand> Commands { get; }
    }

    public class RibbonGroup : IRibbonGroup
    {
        public string Name { get; }
        public string Title { get; }
        public List<IRibbonCommand> Commands { get; }

        public RibbonGroup(string name, string title) 
        {
            Name = name;
            Title = title;
            Commands = new List<IRibbonCommand>();
        }
    }
}
