using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonCustomCommand  : IRibbonCommand
    {
        object Content { get; }
        DataTemplate Template { get; }
    }

    public class RibbonCustomCommand : RibbonCommand, IRibbonCustomCommand
    {
        public object Content { get; }
        public DataTemplate Template { get; }

        public RibbonCustomCommand(string title, Image icon, string description,
            object content, DataTemplate template)
            : base(title, icon, description)
        {
            Content = content;
            Template = template;
        }
    }
}
