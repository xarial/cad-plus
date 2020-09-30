//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    [Flags]
    public enum StartupOptions_e 
    {
        None = 0,

        [Summary("Bypasses all settings")]
        Safe = 1,

        [Summary("Runs host application in background")]
        Background = 2,

        [Summary("Suppresses all popup windows")]
        Silent = 4
    }

    public class BatchRunnerOptions
    {
        public string[] Input { get; set; }
        public string Filter { get; set; }
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
        public string[] Macros { get; set; }
        public AppVersionInfo Version { get; set; }
        public StartupOptions_e StartupOptions { get; set; }
    }
}
