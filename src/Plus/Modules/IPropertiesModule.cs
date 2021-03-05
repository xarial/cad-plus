//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Modules
{
    public enum PropertiesModuleGroup_e
    {
        Input,
        Scope
    }

    public interface IPropertiesModule : IModule
    {
        void AddCommands(PropertiesModuleGroup_e group, params IRibbonCommand[] cmd);
    }
}
