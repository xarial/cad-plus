//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.CustomToolbar.Enums
{
    [Flags]
    public enum MacroScope_e
    {
        [Summary("No Open Documents")]
        Application = 1 << 0,

        Part = 1 << 1,
        Assembly = 1 << 2,
        Drawing = 1 << 3,

        [Title("All Documents")]
        [Summary("All Documents (Part, Assembly, Drawing)")]
        AllDocuments = Part | Assembly | Drawing,

        All = Application | AllDocuments
    }
}
