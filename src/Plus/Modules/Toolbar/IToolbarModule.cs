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

namespace Xarial.CadPlus.Plus.Modules.Toolbar
{
    public interface IToolbarModule : IModule
    {
        event MacroRunningDelegate MacroRunning;

        void RegisterIconsProvider(IIconsProvider provider);
    }
}
