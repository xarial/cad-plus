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
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Structures;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IMacroRunnerExService
    {
        void RunMacro(IXApplication app, string macroPath, MacroEntryPoint entryPoint,
            MacroRunOptions_e opts, string args, IXDocument doc);
    }
}
