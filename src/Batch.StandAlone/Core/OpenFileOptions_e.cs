//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    [Flags]
    public enum OpenFileOptions_e 
    {
        Default = 0,

        [Summary("Suppresses all message boxes")]
        Silent = 1,

        [Summary("Opens all documents read-only")]
        [Title("Read Only")]
        ReadOnly = 2,

        [Summary("Opens documents in the rapid mode. Some of the APIs might be unavailable")]
        Rapid = 4,

        [Summary("Opens documents invisible. Some of the APIs might be unavailable")]
        Invisible = 8,

        [Title("Forbig Upgrade")]
        [Summary("Forbid upgrading of documents to a new version")]
        ForbidUpgrade = 16
    }
}
