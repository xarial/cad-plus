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
    [Verb("file", HelpText = "Managing xBatch files")]
    public class FileOptions
    {
        [Option('o', "open", Required = false, HelpText = "Starts application and opens specified file")]
        public string FilePath { get; set; }

        [Option('n', "new", Required = false, HelpText = "Starts application and creates new file")]
        public bool CreateNew { get; set; }
    }

    public interface IArguments 
    {
        BatchJob GetOptions(IApplicationProvider appProvider);
    }

    [Verb("job", HelpText = "Accessing job files")]
    public class JobOptions : IArguments
    {
        private BatchJob m_Options;

        [Option('r', "run", Required = true, HelpText = "Full path to *.xbatch file to run")]
        public string JobFilePath 
        {
            set 
            {
                m_Options = BatchJob.FromFile(value);
            }
        }

        public BatchJob GetOptions(IApplicationProvider appProvider) 
        {
            m_Options.Version = appProvider.ParseVersion(m_Options.Version?.Id);
            return m_Options;
        }
    }

    [Verb("run", isDefault: true, HelpText = "Runs jobs by specifying parameters")]
    public class RunOptions : IArguments
    {
        private BatchJob m_Options;

        public BatchJob GetOptions(IApplicationProvider appProvider) 
        {
            m_DeferredSetters.ForEach(s => s.Invoke(appProvider));
            return m_Options;
        }

        private List<Action<IApplicationProvider>> m_DeferredSetters;

        public RunOptions() 
        {
            m_DeferredSetters = new List<Action<IApplicationProvider>>();
            m_Options = new BatchJob();
        }
        
        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process")]
        public IEnumerable<string> Input 
        {
            set => m_Options.Input = value?.ToArray();
        }

        [Option('f', "filters", Required = false, HelpText = "Filters to extract input files, if input parameter contains directories. Default (all files): *.*")]
        public string[] Filters
        {
            set => m_Options.Filters = value;
        }

        [Option('m', "macros", Required = true, HelpText = "List of macros to run")]
        public IEnumerable<string> Macros 
        {
            set => m_Options.Macros = value?.ToArray();
        }

        [Option('e', "error", Required = false, HelpText = "If this option is used execution will continue if any of the macros failed to process, otherwise the process will terminate. Default: true")]
        public bool ContinueOnError 
        {
            set => m_Options.ContinueOnError = value;
        }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. running macro on a single file). Default: 600 seconds")]
        public int Timeout
        {
            set => m_Options.Timeout = value;
        }
        
        [Option('s', "startup", Required = false, HelpText = "Specifies the startup options (silent, background, safe) for the host application. Defaul: silent and safe")]
        public IEnumerable<StartupOptions_e> StartupOptions 
        {
            set 
            {
                if (value?.Any() == true)
                {
                    m_Options.StartupOptions = value.Aggregate((StartupOptions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('v', "hostversion", Required = false, HelpText = "Version of host application. Default: oldest")]
        public string Version
        {
            set => m_DeferredSetters.Add(new Action<IApplicationProvider>(p => m_Options.Version = p.ParseVersion(value)));
        }

        [Option('o', "open", Required = false, HelpText = "Specifies options (silent, readonly, rapid) for the file opening. Default: silent")]
        public IEnumerable<OpenFileOptions_e> OpenFileOptions
        {
            set
            {
                if (value?.Any() == true)
                {
                    m_Options.OpenFileOptions = value.Aggregate((OpenFileOptions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('b', "batch", Required = false, HelpText = "maximum number of files to process in the single session of CAD application before restarting. Default: 25")]
        public int BatchSize
        {
            set => m_Options.BatchSize = value;
        }
    }
}
