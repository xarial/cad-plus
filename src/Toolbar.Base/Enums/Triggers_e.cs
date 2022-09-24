//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.ComponentModel;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.CustomToolbar.Enums
{
    [Flags]
    public enum Triggers_e
    {
        [Summary("Disabled command")]
        [Title("None (Disabled)")]
        None = 0,

        [Summary("Invoked by clicking button in the toolbar")]
        [Title("Button")]
        Button = 1 << 0,

        [Title("Toggle Button")]
        ToggleButton = Button | Toggle,

        [Title("Application Start")]
        ApplicationStart = 1 << 1,

        [Title("New Document")]
        DocumentNew = 1 << 2,

        [Title("Open Document")]
        DocumentOpen = 1 << 3,

        [Title("Activate Document")]
        [Summary("Invoked when document is opened in its own window")]
        DocumentActivated = 1 << 4,

        [Title("Save Document")]
        DocumentSave = 1 << 5,

        [Title("Close Document")]
        DocumentClose = 1 << 6,

        [Title("New/Clear Selection")]
        Selection = 1 << 7,

        [Title("Change Configuration/Sheet")]
        ConfigurationSheetChange = 1 << 8,

        [Title("Rebuild")]
        [Summary("Invoked when document is regenerated")]
        Rebuild = 1 << 9,

        [Browsable(false)]
        Toggle = 1 << 10,

        [Title("New Document Modeling Started")]
        [Summary("Invoked when first feature, component or drawing view is added to the newly created document")]
        ModelingStarted = 1 << 11
    }
}
