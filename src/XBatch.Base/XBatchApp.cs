//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.ViewModels;

namespace Xarial.CadPlus.XBatch.Base
{
    public abstract class XBatchApp : MixedApplication<Arguments>
    {
        private FileOptions m_StartupOptions;

        protected override void OnAppStart()
        {
            this.StartupUri = new Uri("/XBatch.Base;component/MainWindow.xaml", UriKind.Relative);
        }

        protected override void OnWindowStarted()
        {
            if (m_StartupOptions != null) 
            {
                var vm = (BatchManagerVM)this.MainWindow.DataContext;

                if (!string.IsNullOrEmpty(m_StartupOptions.FilePath)) 
                {
                    vm.OpenDocument(m_StartupOptions.FilePath);
                }

                if (m_StartupOptions.CreateNew) 
                {
                    vm.NewDocument();
                }
            }
        }

        protected override Task RunConsole(Arguments args)
        {
            return RunConsoleBatch(args);
        }

        private async Task RunConsoleBatch(Arguments args)
        {
            var appProvider = GetApplicationProvider();

            var opts = args.GetOptions(appProvider);

            using (var batchRunner = new BatchRunner(appProvider, Console.Out, new ConsoleProgressWriter()))
            {
                await batchRunner.BatchRun(opts).ConfigureAwait(false);
            }
        }

        protected override void TryExtractCliArguments(Parser parser, string[] input, 
            out Arguments args, out bool hasArguments, out bool hasError)
        {
            args = default;
            hasError = false;
            hasArguments = false;

            if (input.Any())
            {
                Arguments argsLocal = default;
                bool hasErrorLocal = false;
                bool hasArgumentsLocal = false;

                parser.ParseArguments<FileOptions, Arguments>(input)
                    .WithParsed<Arguments>(a => { argsLocal = a; hasArgumentsLocal = true; })
                    .WithParsed<FileOptions>(a => m_StartupOptions = a)
                    .WithNotParsed(err => hasErrorLocal = true);

                args = argsLocal;
                hasError = hasErrorLocal;
                hasArguments = hasArgumentsLocal;
            }
        }
        
        public abstract IApplicationProvider GetApplicationProvider();
    }
}
