using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonSwitchCommand : IRibbonToggleCommand 
    {
    }

    public class RibbonSwitchCommand : RibbonToggleCommand, IRibbonSwitchCommand
    {
        public RibbonSwitchCommand(string title, Image icon, Func<bool> getter, Action<bool> setter)
            : base(title, icon, getter, setter)
        {
        }
    }
}
