//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
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
        private static Window m_Window;
        private static ApplicationLauncher<ExportApplication, Arguments, MainWindow> m_AppLauncher;
        private static ConsoleProgressWriter m_ProgressWriter;

        [STAThread]
        static void Main(string[] args)
        {
            m_ProgressWriter = new ConsoleProgressWriter();
            m_AppLauncher = new ApplicationLauncher<ExportApplication, Arguments, MainWindow>(new Initiator());
            m_AppLauncher.RunConsoleAsync += OnRunConsoleAsync;
            m_AppLauncher.WindowCreated += OnWindowCreated;
            m_AppLauncher.ConfigureServices += OnConfigureServices;
            m_AppLauncher.Start(args);
        }

        private static void OnConfigureServices(IContainerBuilder builder, Arguments args)
        {
            builder.RegisterSelfSingleton<ExporterVM>();
            builder.RegisterSingleton<IExporterModel, ExporterModel>();
            builder.RegisterSingleton<IAboutService, AboutService>();
            builder.RegisterSingleton<IParentWindowProvider, ParentWindowProvider>().UsingFactory(() => new ParentWindowProvider(() => m_Window));
            builder.RegisterSingleton<IJobManager, JobManager>().UsingInitializer(x => x.Init());
        }

        private static void OnWindowCreated(MainWindow window, Arguments args)
        {
            m_Window = window;
            
            var vm = m_AppLauncher.Container.GetService<ExporterVM>();

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

            var jobMgr = m_AppLauncher.Container.GetService<IJobManager>();

            using (var exporter = new Exporter(jobMgr, opts))
            {
                exporter.Log += OnLog;
                exporter.ProgressChanged += OnProgressChanged;
                exporter.JobSet += OnJobSet;
                exporter.JobCompleted += OnJobCompleted;

                await exporter.ExecuteAsync(default).ConfigureAwait(false);
            }
        }

        private static void OnJobCompleted(TimeSpan duration) => m_ProgressWriter.ReportCompleted(duration);
        private static void OnJobSet(IJobItem[] scope, DateTime startTime) => m_ProgressWriter.SetJobScope(scope, startTime);
        private static void OnProgressChanged(IJobItem file, bool result) => m_ProgressWriter.ReportProgress(file, result);
        private static void OnLog(string msg) => m_ProgressWriter.Log(msg);
    }
}
