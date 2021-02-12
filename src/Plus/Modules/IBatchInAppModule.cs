using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Plus.Modules
{
    public interface IBatchInAppModule
    {
        event ProcessBatchInputDelegate ProcessInput;
        void AddCommands(params IRibbonCommand[] cmd);
    }
}
