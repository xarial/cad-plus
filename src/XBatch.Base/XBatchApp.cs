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
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;

namespace Xarial.CadPlus.XBatch.Base
{
    public abstract class XBatchApp : MixedApplication<Arguments>
    {
        protected override void OnAppStart()
        {
            this.StartupUri = new Uri("/XBatch.Base;component/MainWindow.xaml", UriKind.Relative);
        }

        protected override async Task RunConsole(Arguments args)
        {
            var appProvider = GetApplicationProvider();

            var opts = new BatchRunnerOptions()
            {
                Input = args.Input?.ToArray(),
                Filter = args.Filter,
                Macros = args.Macros?.ToArray(),
                Timeout = args.Timeout,
                ContinueOnError = args.ContinueOnError,
                StartupOptions = args.StartupOptions,
                Version = appProvider.ParseVersion(args.Version)
            };

            using (var batchRunner = new BatchRunner(appProvider, Console.Out, new ConsoleProgressWriter()))
            {
                await batchRunner.BatchRun(opts).ConfigureAwait(false);
            }
        }

        public abstract IApplicationProvider GetApplicationProvider();
    }
}
