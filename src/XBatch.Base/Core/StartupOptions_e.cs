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
    public enum StartupOptions_e 
    {
        Default = 0,

        [Summary("Bypasses all settings")]
        Safe = 1,

        [Summary("Runs host application in background")]
        Background = 2,

        [Summary("Suppresses all popup windows")]
        Silent = 4,

        [Summary("Hides the main window of the application")]
        Hidden = 8
    }
}
