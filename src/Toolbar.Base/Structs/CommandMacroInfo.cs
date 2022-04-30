//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json;
using System;
using System.Xml.Serialization;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.Plus.Modules;

namespace Xarial.CadPlus.CustomToolbar.Structs
{
    public class CommandMacroInfo : CommandItemInfo, ICommandMacroInfo
    {
        [JsonIgnore, XmlIgnore]
        IMacroStartFunction ICommandMacroInfo.EntryPoint => EntryPoint;

        public string MacroPath { get; set; }
        public MacroStartFunction EntryPoint { get; set; }
        public MacroScope_e Scope { get; set; } = MacroScope_e.All;
        public Triggers_e Triggers { get; set; } = Triggers_e.Button;
        public bool UnloadAfterRun { get; set; } = true;
        public Location_e Location { get; set; } = Location_e.Toolbar | Location_e.Menu;

        public string Arguments { get; set; }

        public ToggleButtonStateCode_e ToggleButtonStateCodeType { get; set; } = ToggleButtonStateCode_e.None;
        public string ToggleButtonStateCode { get; set; } = "";
        public bool ResolveButtonStateCodeOnce { get; set; } = true;

        internal CommandMacroInfo Clone()
            => new CommandMacroInfo()
            {
                Description = Description,
                IconPath = IconPath,
                Id = Id,
                Title = Title,
                MacroPath = MacroPath,
                EntryPoint = EntryPoint,
                Scope = Scope,
                Triggers = Triggers,
                UnloadAfterRun = UnloadAfterRun,
                Location = Location,
                Arguments = Arguments,
                ToggleButtonStateCodeType = ToggleButtonStateCodeType,
                ToggleButtonStateCode = ToggleButtonStateCode,
                ResolveButtonStateCodeOnce = ResolveButtonStateCodeOnce
            };
    }
}