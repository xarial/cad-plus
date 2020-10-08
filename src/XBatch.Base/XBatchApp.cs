//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
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

        protected override Task RunConsole(Arguments args)
        {
            return RunConsoleBatch(args);
        }

        private async Task RunConsoleBatch(Arguments args)
        {
            var appProvider = GetApplicationProvider();

            var opts = args.Options;

            using (var batchRunner = new BatchRunner(appProvider, Console.Out, new ConsoleProgressWriter()))
            {
                await batchRunner.BatchRun(opts).ConfigureAwait(false);
            }
        }

        protected override Arguments CreateArguments() => new Arguments(GetApplicationProvider());

        public abstract IApplicationProvider GetApplicationProvider();
    }
}
