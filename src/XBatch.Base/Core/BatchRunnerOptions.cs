//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchRunnerOptions
    {
        public string[] Input { get; set; }
        public string Filter { get; set; }
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
        public string[] Macros { get; set; }
        public AppVersionInfo Version { get; set; }
        public bool RunInBackground { get; set; }
    }
}
