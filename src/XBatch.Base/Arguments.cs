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
using Xarial.CadPlus.XBatch.Base.Core;

namespace Xarial.CadPlus.XBatch.Base
{
    public class Arguments
    {
        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process")]
        public IEnumerable<string> Input { get; set; }

        [Option('f', "filter", Required = false, HelpText = "Filter to extract input files, if input parameter contains directories")]
        public string Filter { get; set; }

        [Option('m', "macros", Required = true, HelpText = "List of macros to run")]
        public IEnumerable<string> Macros { get; set; }

        [Option('e', "error", Required = false, HelpText = "If this option is used execution will continue if any of the macros failed to process, otherwise the process will terminate")]
        public bool ContinueOnError { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. running macro on a single file)")]
        public int Timeout { get; set; }

        public StartupOptions_e StartupOptions
        {
            get
            {
                return _StartupOptions.Aggregate((StartupOptions_e)0, (o, c) => o | c);
            }
        }

        [Option('s', "startup", Required = false, HelpText = "Specifies the startup options (silent, background, safe) for the host application")]
        public IEnumerable<StartupOptions_e> _StartupOptions { get; set; }
        
        [Option('v', "hostversion", Required = false, HelpText = "Version of host application")]
        public string Version { get; set; }
    }
}
