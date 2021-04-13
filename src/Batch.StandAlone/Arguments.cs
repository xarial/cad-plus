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
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.XBatch.Base.Core;

namespace Xarial.CadPlus.XBatch.Base
{
    [Verb("file", HelpText = "Managing Batch+ files")]
    public class FileOptions
    {
        [Option('o', "open", Required = false, HelpText = "Starts application and opens specified file")]
        public string FilePath { get; set; }

        [Option('n', "new", Required = false, HelpText = "Starts application and creates new file")]
        public bool CreateNew { get; set; }

        [Option('a', "appId", Required = false, HelpText = "Id of application if new document is created")]
        public string ApplicationId { get; set; }
    }

    public class BatchArguments 
    {
        public BatchJob Job { get; protected set; }
    }

    [Verb("job", HelpText = "Accessing job files")]
    public class JobOptions : BatchArguments
    {
        [Option('r', "run", Required = true, HelpText = "Full path to *.batchplus file to run")]
        public string JobFilePath 
        {
            set 
            {
                Job = BatchJob.FromFile(value);
            }
        }
    }

    [Verb("run", isDefault: true, HelpText = "Runs jobs by specifying parameters")]
    public class RunOptions : BatchArguments
    {
        public RunOptions() 
        {
            Job = new BatchJob();

            //TODO: make this a parameter once more providers are implemented
            Job.ApplicationId = Plus.CadApplicationIds.SolidWorks;
        }
        
        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process")]
        public IEnumerable<string> Input 
        {
            set => Job.Input = value?.ToArray();
        }

        [Option('f', "filters", Required = false, HelpText = "Filters to extract input files, if input parameter contains directories. Default (all files): *.*")]
        public IEnumerable<string> Filters
        {
            set => Job.Filters = value.ToArray();
        }

        [Option('m', "macros", Required = true, HelpText = "List of macros to run")]
        public IEnumerable<string> Macros 
        {
            //TODO: add support for args
            set => Job.Macros = value.Select(m => new MacroData() { FilePath = m })?.ToArray();
        }

        [Option('e', "error", Required = false, HelpText = "If this option is used execution will continue if any of the macros failed to process, otherwise the process will terminate. Default: true")]
        public bool ContinueOnError 
        {
            set => Job.ContinueOnError = value;
        }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. running macro on a single file). Default: 600 seconds")]
        public int Timeout
        {
            set => Job.Timeout = value;
        }
        
        [Option('s', "startup", Required = false, HelpText = "Specifies the startup options (silent, background, safe, hidden) for the host application. Defaul: silent and safe")]
        public IEnumerable<StartupOptions_e> StartupOptions 
        {
            set 
            {
                if (value?.Any() == true)
                {
                    Job.StartupOptions = value.Aggregate((StartupOptions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('v', "hostversion", Required = false, HelpText = "Version of host application. Default: oldest")]
        public string Version
        {
            set => Job.VersionId = value;
        }

        [Option('o', "open", Required = false, HelpText = "Specifies options (silent, readonly, rapid, invisible, forbidupgrade) for the file opening. Default: silent")]
        public IEnumerable<OpenFileOptions_e> OpenFileOptions
        {
            set
            {
                if (value?.Any() == true)
                {
                    Job.OpenFileOptions = value.Aggregate((OpenFileOptions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('a', "actions", Required = false, HelpText = "Specifies actions (autosavedocuments) to perform for documents. Default: none")]
        public IEnumerable<Actions_e> Actions 
        {
            set 
            {
                if (value?.Any() == true)
                {
                    Job.Actions = value.Aggregate((Actions_e)0, (o, c) => o | c);
                }
            }
        }

        [Option('b', "batch", Required = false, HelpText = "Maximum number of files to process in the single session of CAD application before restarting. Default: 25")]
        public int BatchSize
        {
            set => Job.BatchSize = value;
        }
    }
}
