//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Plus.Modules.Batch
{
    public interface IBatchInAppModule : IModule
    {
        event ProcessInAppBatchInputDelegate ProcessInput;
        void AddCommands(BatchModuleGroup_e group, params IRibbonCommand[] cmd);
    }
}
