//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
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

        [STAThread]
        static void Main(string[] args)
        {
            m_AppLauncher = new ApplicationLauncher<ExportApplication, Arguments, MainWindow>(new Initiator());
            m_AppLauncher.RunConsoleAsync += OnRunConsoleAsync;
            m_AppLauncher.WindowCreated += OnWindowCreated;
            m_AppLauncher.ConfigureServices += OnConfigureServices;
            m_AppLauncher.Start(args);
        }

        private static void OnConfigureServices(ContainerBuilder builder, Arguments args)
        {
            builder.RegisterType<ExporterVM>();
            builder.RegisterType<ExporterModel>().As<IExporterModel>();
            builder.RegisterType<AboutService>().As<IAboutService>();
            builder.Register<Window>(c => m_Window);
            builder.RegisterType<JobManager>().As<IJobManager>()
                .SingleInstance()
                .OnActivating(x => x.Instance.Init());
        }

        private static void OnWindowCreated(MainWindow window, Arguments args)
        {
            m_Window = window;
            
            var vm = m_AppLauncher.Container.Resolve<ExporterVM>();

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

            var jobMgr = m_AppLauncher.Container.Resolve<IJobManager>();

            using (var exporter = new Exporter(Console.Out, jobMgr, new ConsoleProgressWriter()))
            {
                await exporter.Export(opts).ConfigureAwait(false);
            }
        }
    }
}
