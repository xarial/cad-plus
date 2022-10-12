//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Modules.Toolbar
{
    public class MacroRunningArguments
    {
        public ICommandMacroInfo MacroInfo { get; }
        public IXDocument TargetDocument { get; }
        public bool Cancel { get; set; }

        public MacroRunningArguments(ICommandMacroInfo macroInfo, IXDocument targetDoc)
        {
            MacroInfo = macroInfo;
            TargetDocument = targetDoc;
        }
    }
}
