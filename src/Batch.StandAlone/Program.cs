//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Batch.Base;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.XCad.Base;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus;
using System.Windows;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Batch.StandAlone.Services;

namespace Xarial.CadPlus.Batch.StandAlone
{
    class Program
    {
        private const int MAX_RETRIES = 2;
        
        private static Base.FileOptions m_StartupOptions;

        private static ApplicationLauncher<BatchApplication, BatchArguments, MainWindow> m_AppLauncher;

        private static BatchManagerVM m_BatchManager;

        private static Window m_Window;

        private static ConsoleProgressWriter m_ProgressWriter;

        [STAThread]
        static void Main(string[] args)
        {
            m_ProgressWriter = new ConsoleProgressWriter();

            m_AppLauncher = new ApplicationLauncher<BatchApplication, BatchArguments, MainWindow>(new Initiator());
            m_AppLauncher.ConfigureServices += OnConfigureServices;
            m_AppLauncher.ParseArguments += OnParseArguments;
            m_AppLauncher.WriteHelp += OnWriteHelp;
            m_AppLauncher.WindowCreated += OnWindowCreated;
            m_AppLauncher.RunConsoleAsync += OnRunConsoleAsync;
            m_AppLauncher.Start(args);
        }

        private static Task OnRunConsoleAsync(BatchArguments args) => RunConsoleBatch(args);

        private static async Task RunConsoleBatch(BatchArguments args)
        {
            try
            {
                var batchRunFact = m_AppLauncher.Container.GetService<IBatchRunJobExecutorFactory>();

                using (var batchRunner = batchRunFact.Create(args.Job))
                {
                    batchRunner.Log += OnLog;
                    batchRunner.ProgressChanged += OnProgressChanged;
                    batchRunner.JobSet += OnJobSet;
                    batchRunner.JobCompleted += OnJobCompleted;

                    await batchRunner.ExecuteAsync(default).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ParseUserError());
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        private static void OnJobCompleted(TimeSpan duration) => m_ProgressWriter.ReportCompleted(duration);
        private static void OnJobSet(IJobItem[] scope, DateTime startTime) => m_ProgressWriter.SetJobScope(scope, startTime);
        private static void OnProgressChanged(IJobItem file, double progress, bool result) => m_ProgressWriter.ReportProgress(file, progress, result);
        private static void OnLog(string msg) => m_ProgressWriter.Log(msg);

        private static void OnConfigureServices(IContainerBuilder builder, BatchArguments args)
        {
            builder.RegisterSingleton<IRecentFilesManager, RecentFilesManager>();
            builder.RegisterSingleton<IBatchRunnerModel, BatchRunnerModel>();
            builder.RegisterSingleton<IBatchRunJobExecutorFactory, BatchRunJobExecutorFactory>();
            builder.RegisterSelfSingleton<BatchManagerVM>();
            builder.RegisterSingleton<IJobContectResilientWorkerFactory, PollyJobContectResilientWorkerFactory>().UsingParameters(Parameter<int>.Any(MAX_RETRIES));

            builder.RegisterSingleton<IPopupKillerFactory, PopupKillerFactory>();
            builder.RegisterSingleton<IBatchDocumentVMFactory, BatchDocumentVMFactory>();
            builder.RegisterSingleton<IAboutService, AboutService>();
            builder.RegisterSingleton<IParentWindowProvider, ParentWindowProvider>().UsingFactory(() => new ParentWindowProvider(() => m_Window));

            builder.Register<IBatchApplicationProxy, BatchApplicationProxy>();

            builder.RegisterAdapter<IApplication, IBatchApplication>(LifetimeScope_e.Singleton);

            builder.RegisterSingleton<IJobApplicationProvider, JobApplicationProvider>();

            builder.RegisterSingleton<IXCadMacroProvider, XCadMacroProvider>();

            builder.RegisterSingleton<IJournalExporter, JournalTextExporter>().AsCollectionItem();
            builder.RegisterSingleton<IResultsSummaryExcelExporter, ResultsSummaryExcelExporter>().AsCollectionItem();
        }

        private static void OnWindowCreated(MainWindow window, BatchArguments args)
        {
            try 
            {
                m_Window = window;

                m_BatchManager = m_AppLauncher.Container.GetService<BatchManagerVM>();
                window.Closing += OnWindowClosing;
                window.DataContext = m_BatchManager;
                
                m_BatchManager.ParentWindow = window;

                if (m_StartupOptions != null)
                {
                    if (!string.IsNullOrEmpty(m_StartupOptions.FilePath))
                    {
                        m_BatchManager.OpenDocument(m_StartupOptions.FilePath);
                    }

                    if (m_StartupOptions.CreateNew)
                    {
                        m_BatchManager.CreateDocument(m_StartupOptions.ApplicationId);
                    }
                }
            }
            catch (Exception ex)
            {
                IMessageService msgSvc;

                try
                {
                    msgSvc = m_AppLauncher.Container.GetService<IMessageService>();
                }
                catch
                {
                    msgSvc = new GenericMessageService();
                }

                msgSvc.ShowError(ex.ParseUserError());
                Environment.Exit(1);
            }
        }

        private static void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !m_BatchManager.CanClose();
        }

        private static bool OnParseArguments(string[] input, Parser parser,
            ref BatchArguments args, ref bool createConsole)
        {
            args = default;
            var hasError = false;
            
            if (input.Any())
            {
                BatchArguments argsLocal = default;
                bool createConsoleLocal = false;

                CreateParserResult(parser, input)
                    .WithParsed<RunOptions>(a => { argsLocal = a; createConsoleLocal = true; })
                    .WithParsed<JobOptions>(a => { argsLocal = a; createConsoleLocal = true; })
                    .WithParsed<Base.FileOptions>(a => m_StartupOptions = a)
                    .WithNotParsed(err => { hasError = true; createConsoleLocal = true; });

                args = argsLocal;
                createConsole = createConsoleLocal;
            }

            return !hasError;
        }

        private static void OnWriteHelp(Parser parser, string[] args)
        {
            CreateParserResult(parser, args);
        }

        private static ParserResult<object> CreateParserResult(Parser parser, string[] args)
            => parser.ParseArguments<Base.FileOptions, RunOptions, JobOptions>(args);
    }
}
