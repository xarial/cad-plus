using System;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.XToolbar.Enums
{
    [Flags]
    public enum MacroScope_e
    {
        [Summary("No Open Documents")]
        Application = 1,

        Part = 2,
        Assembly = 4,
        Drawing = 8,

        [Title("All Documents")]
        [Summary("All Documents (Part, Assembly, Drawing)")]
        AllDocuments = Part | Assembly | Drawing,

        All = Application | AllDocuments
    }
}
