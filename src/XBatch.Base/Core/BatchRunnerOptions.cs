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
        Default = 0,

        [Summary("Bypasses all settings")]
        Safe = 1,

        [Summary("Runs host application in background")]
        Background = 2,

        [Summary("Suppresses all popup windows")]
        Silent = 4
    }

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
        Rapid = 4
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
        public OpenFileOptions_e OpenFileOptions { get; set; }

        public BatchRunnerOptions() 
        {
            Filter = "*.*";
            Timeout = 600;
            ContinueOnError = true;
            StartupOptions = StartupOptions_e.Silent | StartupOptions_e.Safe;
            OpenFileOptions = OpenFileOptions_e.Silent;
        }
    }
}
