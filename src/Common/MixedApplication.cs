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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Init;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Hosts;

namespace Xarial.CadPlus.Common
{
    internal static class ConsoleHandler
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr handle);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetFileType(IntPtr handle);

        private const int FILE_TYPE_DISK = 0x0001;
        private const int FILE_TYPE_PIPE = 0x0003;

        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        
        internal static void Attach()
        {
            //need to call before AttachConsoel so the output can be redirected
            var outHandle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (IsOutputRedirected(outHandle))
            {
                var outWriter = Console.Out;
            }

            bool errorRedirected = IsOutputRedirected(GetStdHandle(STD_ERROR_HANDLE));

            if (errorRedirected) 
            {
                var errWriter = Console.Error;
            }

            AttachConsole(-1);

            if (!errorRedirected)
            {
                SetStdHandle(STD_ERROR_HANDLE, outHandle);
            }
        }

        private static bool IsOutputRedirected(IntPtr handle)
        {
            var fileType = GetFileType(handle);

            return fileType == FILE_TYPE_DISK || fileType == FILE_TYPE_PIPE;
        }
    }

    public abstract class MixedApplication<TCliArgs> : Application
    {
        private bool m_IsStartWindowCalled;

        public IHost Host { get; private set; }

        protected IContainer m_Container;

        private readonly IApplication m_App;
        private readonly IInitiator m_Initiator;

        protected MixedApplication(IApplication app) 
        {
            m_App = app;
            m_Initiator = new Initiator();
        }

        protected virtual void OnAppStart()
        {
        }

        protected virtual void TryExtractCliArguments(Parser parser, string[] input, 
            out TCliArgs args, out bool hasArguments, out bool hasError)
        {
            args = default;
            hasError = false;
            hasArguments = false;

            if (input.Any())
            {
                TCliArgs argsLocal = default;
                bool hasErrorLocal = false;

                parser.ParseArguments<TCliArgs>(input)
                    .WithParsed(a => argsLocal = a)
                    .WithNotParsed(err => hasErrorLocal = true);

                args = argsLocal;
                hasError = hasErrorLocal;
                hasArguments = true;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            this.Exit += OnAppExit;

            OnAppStart();

            var parserOutput = new StringBuilder();

            TCliArgs args;
            bool hasArgs;
            bool hasError;

            using (var outputWriter = new StringWriter(parserOutput))
            {
                var parser = new Parser(p =>
                {
                    p.CaseInsensitiveEnumValues = true;
                    p.AutoHelp = true;
                    p.EnableDashDash = true;
                    p.HelpWriter = outputWriter;
                    p.IgnoreUnknownArguments = false;
                });

                TryExtractCliArguments(parser, e.Args, out args, out hasArgs, out hasError);
            }

            var svc = CreateServiceProvider();

            if (hasArgs)
            {
                ConsoleHandler.Attach();

                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                Host = new HostConsole(m_App, svc, m_Initiator);

                var res = false;

                if (!hasError)
                {
                    try
                    {
                        RunConsole(args)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                        res = true;
                    }
                    catch (Exception ex)
                    {
                        //TODO: message exception
                        PrintError(ex.Message);
                    }
                }
                else 
                {
                    Console.Write(parserOutput.ToString());
                }

                Environment.Exit(res ? 0 : 1);
            }
            else
            {
                Host = new HostWpf(m_App, this, svc, m_Initiator);
                base.OnStartup(e);
            }
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            m_Container?.Dispose();
        }

        private IServiceProvider CreateServiceProvider() 
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<GenericMessageService>()
                .As<IMessageService>()
                .WithParameter(new TypedParameter(typeof(string), "CAD+"));

            OnConfigureServices(builder);

            m_Container = builder.Build();

            return new ServiceProvider(m_Container);
        }

        protected virtual void OnConfigureServices(ContainerBuilder builder) 
        {
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!m_IsStartWindowCalled)
            {
                m_IsStartWindowCalled = true;
                OnWindowStarted();
            }
        }

        protected virtual void OnWindowStarted() 
        {
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO: log
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //TODO: log
            e.Handled = true;
        }

        protected abstract Task RunConsole(TCliArgs args);

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
