//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.XToolbar.Enums
{
    [Flags]
    public enum Triggers_e
    {
        [Summary("Disabled command")]
        [Title("None")]
        None = 0,

        [Summary("Invoked by clicking button in the toolbar")]
        [Title("Button")]
        Button = 1 << 0,

        [Title("Application Start")]
        ApplicationStart = 1 << 1,

        [Title("New Document")]
        DocumentNew = 1 << 2,

        [Title("Open Document")]
        DocumentOpen = 1 << 3,

        [Title("Activate Document")]
        DocumentActivated = 1 << 4,

        [Title("Save Document")]
        DocumentSave = 1 << 5,

        [Title("Close Document")]
        DocumentClose = 1 << 6,

        [Title("New Selection")]
        NewSelection = 1 << 7,

        [Title("Change Configuration")]
        ConfigurationChange = 1 << 8,

        [Title("Rebuild")]
        Rebuild = 1 << 9
    }
}
