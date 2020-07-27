//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.CadPlus.XToolbar.Enums;

namespace Xarial.CadPlus.XToolbar.Structs
{
    public class CommandMacroInfo : CommandItemInfo
    {
        public string MacroPath { get; set; }
        public MacroStartFunction EntryPoint { get; set; }
        public MacroScope_e Scope { get; set; } = MacroScope_e.All;
        public Triggers_e Triggers { get; set; } = Triggers_e.Button;
    }
}