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
        internal BatchRunnerOptions Options { get; }

        private readonly IApplicationProvider m_AppProvider;

        public Arguments(IApplicationProvider appProvider) 
        {
            Options = new BatchRunnerOptions();
            m_AppProvider = appProvider;
        }

        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process")]
        public IEnumerable<string> Input 
        {
            set => Options.Input = value?.ToArray();
        }

        [Option('f', "filter", Required = false, HelpText = "Filter to extract input files, if input parameter contains directories. Default (all files): *.*")]
        public string Filter 
        {
            set => Options.Filter = value;
        }

        [Option('m', "macros", Required = true, HelpText = "List of macros to run")]
        public IEnumerable<string> Macros 
        {
            set => Options.Macros = value?.ToArray();
        }

        [Option('e', "error", Required = false, HelpText = "If this option is used execution will continue if any of the macros failed to process, otherwise the process will terminate. Default: true")]
        public bool ContinueOnError 
        {
            set => Options.ContinueOnError = value;
        }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. running macro on a single file). Defauult: 600 seconds")]
        public int Timeout
        {
            set => Options.Timeout = value;
        }
        
        [Option('s', "startup", Required = false, HelpText = "Specifies the startup options (silent, background, safe) for the host application. Defaul: silent and safe")]
        public IEnumerable<StartupOptions_e> StartupOptions 
        {
            set 
            {
                if (value?.Any() == true)
                {
                    Options.StartupOptions = value.Aggregate((StartupOptions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('v', "hostversion", Required = false, HelpText = "Version of host application. Default: oldest")]
        public string Version
        {
            set => Options.Version = m_AppProvider.ParseVersion(value);
        }

        [Option('o', "open", Required = false, HelpText = "Specifies options (silent, readonly, rapid) for the file opening. Default: silent")]
        public IEnumerable<OpenFileOptions_e> OpenFileOptions
        {
            set
            {
                if (value?.Any() == true)
                {
                    Options.OpenFileOptions = value.Aggregate((OpenFileOptions_e)0, (o, c) => o | c);
                }
            }
        }
    }
}
