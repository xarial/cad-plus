//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
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
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.Base.Services;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.XCad.Base;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Batch.StandAlone
{
    class Program
    {
        private const int MAX_RETRIES = 2;

        private static IBatchApplication m_BatchApp;
        private static IBatchApplicationProxy m_BatchAppProxy;

        private static XBatch.Base.FileOptions m_StartupOptions;

        private static ApplicationLauncher<BatchArguments, MainWindow> m_AppLauncher;

        private static BatchManagerVM m_BatchManager;

        [STAThread]
        static void Main(string[] args)
        {
            m_BatchAppProxy = new BatchApplicationProxy();
            m_BatchApp = new BatchApplication(m_BatchAppProxy);

            m_AppLauncher = new ApplicationLauncher<BatchArguments, MainWindow>(m_BatchApp, new Initiator());
            m_AppLauncher.ConfigureServices += OnConfigureServices;
            m_AppLauncher.ParseArguments += OnParseArguments;
            m_AppLauncher.WriteHelp += OnWriteHelp;
            m_AppLauncher.WindowCreated += OnWindowCreated;
            m_AppLauncher.RunConsoleAsync += OnRunConsoleAsync;
            m_AppLauncher.Start(args);
        }

        private static Task OnRunConsoleAsync(BatchArguments args)
            => RunConsoleBatch(args);

        private static async Task RunConsoleBatch(BatchArguments args)
        {
            using (var batchRunner = m_AppLauncher.Container.Resolve<BatchRunner>(
                new TypedParameter[]
                {
                    new TypedParameter(typeof(BatchJob), args.Job),
                    new TypedParameter(typeof(TextWriter), Console.Out),
                    new TypedParameter(typeof(IProgressHandler), new ConsoleProgressWriter())
                }))
            {
                await batchRunner.BatchRunAsync().ConfigureAwait(false);
            }
        }

        private static void OnConfigureServices(ContainerBuilder builder, BatchArguments args)
        {
            builder.RegisterType<RecentFilesManager>()
                .As<IRecentFilesManager>();

            builder.RegisterType<AppLogger>().As<IXLogger>();
            builder.RegisterType<BatchRunner>();
            builder.RegisterType<BatchRunnerModel>().As<IBatchRunnerModel>();
            builder.RegisterType<BatchRunJobExecutor>().As<IBatchRunJobExecutor>();
            builder.RegisterType<BatchManagerVM>();
            builder.RegisterType<PollyResilientWorker<BatchJobContext>>()
                .As<IResilientWorker<BatchJobContext>>()
                .WithParameter(new TypedParameter(typeof(int), MAX_RETRIES));
            builder.RegisterType<PopupKiller>().As<IPopupKiller>();
            builder.RegisterType<BatchDocumentVM>();

            builder.RegisterInstance(m_BatchApp);
            builder.RegisterInstance(m_BatchAppProxy);

            builder.RegisterAdapter<IBatchApplication, IApplicationProvider[]>(x => x.ApplicationProviders);

            builder.RegisterType<JobManager>().As<IJobManager>()
                .SingleInstance()
                .OnActivating(x => x.Instance.Init());
        }

        private static void OnWindowCreated(MainWindow window, BatchArguments args)
        {
            try 
            {
                m_BatchManager = m_AppLauncher.Container.Resolve<BatchManagerVM>();
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
                    msgSvc = m_AppLauncher.Container.Resolve<IMessageService>();
                }
                catch
                {
                    msgSvc = new GenericMessageService("Batch+");
                }

                msgSvc.ShowError(ex.ParseUserError(out _));
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
                    .WithParsed<XBatch.Base.FileOptions>(a => m_StartupOptions = a)
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
            => parser.ParseArguments<XBatch.Base.FileOptions, RunOptions, JobOptions>(args);
    }
}
