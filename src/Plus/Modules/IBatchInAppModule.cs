using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Plus.Modules
{
    public enum Group_e 
    {
        Input,
        Macros,
        Options
    }

    public interface IBatchInAppModule : IModule
    {
        event ProcessBatchInputDelegate ProcessInput;
        void AddCommands(Group_e group, params IRibbonCommand[] cmd);
    }
}
