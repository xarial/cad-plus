//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.CadPlus.CustomToolbar.Enums;

namespace Xarial.CadPlus.CustomToolbar.Structs
{
    public class CommandMacroInfo : CommandItemInfo
    {
        public string MacroPath { get; set; }
        public MacroStartFunction EntryPoint { get; set; }
        public MacroScope_e Scope { get; set; } = MacroScope_e.All;
        public Triggers_e Triggers { get; set; } = Triggers_e.Button;
        public bool UnloadAfterRun { get; set; } = true;
        public Location_e Location { get; set; } = Location_e.Toolbar | Location_e.Menu;

        public bool IsToggleButton { get; set; } = false;
        public ToggleButtonStateCode_e ToggleButtonStateCodeType { get; set; } = ToggleButtonStateCode_e.None;
        public string ToggleButtonStateCode { get; set; } = "";
        public bool ResolveButtonStateCodeOnce { get; set; } = true;
    }
}