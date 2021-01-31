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
}
