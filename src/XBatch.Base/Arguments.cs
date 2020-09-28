//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.Base
{
    public class Arguments
    {
        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process. These are files which can be opened by eDrawings (e.g. SOLIDWORKS files, CATIA, STEP, DXF/DWG, etc.)")]
        public IEnumerable<string> Input { get; set; }

        [Option("filter", Required = false, HelpText = "Filter to extract input files, if input parameter contains directories")]
        public string Filter { get; set; }

        [Option('m', "macros", Required = true, HelpText = "List of macros to run")]
        public IEnumerable<string> Macros { get; set; }

        [Option('e', "error", Required = false, HelpText = "If this option is used execution will continue if any of the macros failed to process, otherwise the process will terminate")]
        public bool ContinueOnError { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. running macro on a single file)")]
        public int Timeout { get; set; }

        [Option('b', "background", Required = false, HelpText = "If this option is specified application is run in the background")]
        public bool RunInBackground { get; set; }

        [Option('p', "parallel", Required = false, HelpText = "Number of parallel jobs to execute batch operation")]
        public int ParallelJobsCount { get; set; } = 1;

        [Option('v', "version", Required = false, HelpText = "Version of host application")]
        public string Version { get; set; }
    }
}
