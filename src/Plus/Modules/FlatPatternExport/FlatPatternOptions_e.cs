using System;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Plus.Modules.Drawing.FlatPatternExport
{
    [Flags]
    public enum FlatPatternOptions_e
    {
        [Title("Geometry Only")]
        GeometryOnly = 0,

        [Title("Bend Lines")]
        BendLines = 1,

        [Title("Bend Notes")]
        BendNotes = 2,

        [Title("Sketches")]
        Sketches = 4
    }
}
