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
using System.Windows.Interop;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Xport.Core;
using Xarial.CadPlus.Xport.Models;
using Xarial.CadPlus.Xport.ViewModels;

namespace Xarial.CadPlus.Xport
{
    public class ExportApplication : IApplication
    {
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var appLauncher = new ApplicationLauncher<ExportApplication, Arguments, MainWindow>(new Initiator());
            appLauncher.RunConsoleAsync += OnRunConsoleAsync;
            appLauncher.WindowCreated += OnWindowCreated;
            appLauncher.Start(args);
        }

        private static void OnWindowCreated(MainWindow window, Arguments args)
        {
            var vm = new ExporterVM(
                new ExporterModel(),
                new GenericMessageService("eXport+"));

            vm.ParentWindow = window;

            window.DataContext = vm;
        }

        private static Task OnRunConsoleAsync(Arguments args)
            => RunConsoleExporter(args);

        private static async Task RunConsoleExporter(Arguments args)
        {
            var opts = new ExportOptions()
            {
                Input = args.Input?.ToArray(),
                Filter = args.Filter,
                Format = args.Format?.ToArray(),
                Timeout = args.Timeout,
                OutputDirectory = args.OutputDirectory,
                ContinueOnError = args.ContinueOnError,
                Version = args.Version
            };

            using (var exporter = new Exporter(Console.Out, new ConsoleProgressWriter()))
            {
                await exporter.Export(opts).ConfigureAwait(false);
            }
        }
    }
}
