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