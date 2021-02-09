//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Xarial.CadPlus.Plus.Hosts;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Plus.Shared
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
            //need to call before AttachConsole so the output can be redirected
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

    public delegate bool ParseArgumentsDelegate<TCliArgs>(string[] input, Parser parser,
            ref TCliArgs args, ref bool createConsole);

    public delegate void WindowCreatedDelegate<TWindow, TCliArgs>(TWindow window, TCliArgs args)
        where TWindow : Window, new();

    public delegate Task RunConsoleAsyncDelegate<TCliArgs>(TCliArgs args);
    public delegate void RunConsoleDelegate<TCliArgs>(TCliArgs args);

    public delegate void ConfigureServicesDelegate<TCliArgs>(ContainerBuilder builder, TCliArgs args);

    public delegate void WriteHelpDelegate(Parser parser, string[] args);

    /// <summary>
    /// This service allows to run application in mixed mode (either console or WPF)
    /// </summary>
    /// <typeparam name="TCliArgs">Class defining the arguments</typeparam>
    /// <typeparam name="TWindow">WPF window to create</typeparam>
    /// <remarks>To use: create new console project, set the STAThreadAttribute on the main class and change the output type to Window application. Create instance and call Start method</remarks>
    public class ApplicationLauncher<TCliArgs, TWindow>
        where TWindow : Window, new()
    {
        public event WriteHelpDelegate WriteHelp;
        public event ParseArgumentsDelegate<TCliArgs> ParseArguments;
        public event WindowCreatedDelegate<TWindow, TCliArgs> WindowCreated;
        public event RunConsoleAsyncDelegate<TCliArgs> RunConsoleAsync;
        public event RunConsoleDelegate<TCliArgs> RunConsole;
        public event ConfigureServicesDelegate<TCliArgs> ConfigureServices;

        private readonly IApplication m_App;
        private readonly IInitiator m_Initiator;

        private readonly IXLogger m_Logger;
        private readonly IMessageService m_MsgService;

        private bool m_IsStarted;

        public IHost Host { get; private set; }
        private Application m_WpfApp;

        public IContainer Container { get; private set; }

        public ApplicationLauncher(IApplication app, IInitiator initiator)
        {
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            m_Logger = new AppLogger();
            m_MsgService = new GenericMessageService();

            m_App = app;
            m_Initiator = initiator;

            m_IsStarted = false;
        }
    
        public void Start(string[] args) 
        {
            if (!m_IsStarted)
            {
                m_IsStarted = true;

                m_Logger.Log("Starting mixed applicaton");

                var parserOutput = new StringBuilder();

                TCliArgs cliArgs;
                bool createConsole;
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

                    if (IsHelpRequest(args, out string[] normArgs)) 
                    {
                        ConsoleHandler.Attach();

                        if (WriteHelp != null)
                        {
                            WriteHelp.Invoke(parser, normArgs);
                        }
                        else 
                        {
                            parser.ParseArguments<TCliArgs>(normArgs);
                        }
                        
                        Console.Write(parserOutput.ToString());
                        Environment.Exit(0);
                    }

                    TryParseArguments(parser, args, outputWriter, out cliArgs, out createConsole, out hasError);
                }

                var svc = CreateContainerBuilder(cliArgs);

                if (createConsole)
                {
                    ConsoleHandler.Attach();

                    var res = false;

                    if (!hasError)
                    {
                        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                        Host = new HostConsole(m_App, svc, m_Initiator);

                        try
                        {
                            if (RunConsoleAsync != null)
                            {
                                RunConsoleAsync?.Invoke(cliArgs)
                                    .ConfigureAwait(false)
                                    .GetAwaiter()
                                    .GetResult();
                            }
                            else if (RunConsole != null)
                            {
                                RunConsole?.Invoke(cliArgs);
                            }
                            else 
                            {
                                throw new Exception("Subscribe to RunConsole or RunConsoleAsync to run console code");
                            }

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
                    m_WpfApp = new Application();
                    m_WpfApp.DispatcherUnhandledException += OnDispatcherUnhandledException;

                    Host = new HostWpf(m_App, m_WpfApp, svc, m_Initiator, m_Logger);

                    var wnd = new TWindow();
                    WindowCreated?.Invoke(wnd, cliArgs);
                    m_WpfApp.Run(wnd);
                }
            }
            else 
            {
                throw new Exception("Application already started");
            }
        }
        
        private bool IsHelpRequest(string[] args, out string[] normArgs)
        {
            const string HELP_SWITCH = "--help";

            var helpArgs = new string[]
            {
                HELP_SWITCH,
                "/?",
                "\\?",
                "-?",
                "--?"
            };

            var normArgsList = new List<string>();

            var isHelp = false;

            if (args != null)
            {
                foreach (var arg in args) 
                {
                    if (helpArgs.Contains(arg, StringComparer.CurrentCultureIgnoreCase))
                    {
                        isHelp = true;
                    }
                    else 
                    {
                        normArgsList.Add(arg);
                    }
                }
            }
            else 
            {
                normArgs = null;
                return false;
            }

            if (isHelp) 
            {
                normArgsList.Add(HELP_SWITCH);
            }

            normArgs = normArgsList.ToArray();

            return isHelp;
        }

        private void TryParseArguments(Parser parser, string[] input,
            TextWriter errorWriter,
            out TCliArgs args, out bool createConsole, out bool hasError)
        {
            args = default;
            hasError = false;
            createConsole = false;

            if (input.Any())
            {
                TCliArgs argsLocal = default;
                bool hasErrorLocal = false;

                parser.ParseArguments<TCliArgs>(input)
                    .WithParsed(a => argsLocal = a)
                    .WithNotParsed(err => hasErrorLocal = true);

                args = argsLocal;
                hasError = hasErrorLocal;
                createConsole = true;
            }

            if (ParseArguments != null)
            {
                try
                {
                    hasError = !ParseArguments.Invoke(input, parser, ref args, ref createConsole);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    errorWriter.WriteLine(ex.ParseUserError(out _));
                    hasError = true;
                }
            }
        }
        
        private IContainerBuilder CreateContainerBuilder(TCliArgs args)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<GenericMessageService>()
                .As<IMessageService>()
                .WithParameter(new TypedParameter(typeof(string), "CAD+"));

            ConfigureServices?.Invoke(builder, args);

            var contWrapper = new ContainerBuilderWrapper(builder);
            contWrapper.ContainerBuild += OnContainerBuild;

            return contWrapper;
        }

        private void OnContainerBuild(IContainer cont)
        {
            Container = cont;
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("");

            m_Logger.Log("Unhandled domain exception");
            m_Logger.Log(ex);
            m_MsgService.ShowError(ex, "Unknown error");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            m_Logger.Log("Unhandled dispatcher exception");
            m_Logger.Log(e.Exception);
            m_MsgService.ShowError(e.Exception, "Unknown error");

            e.Handled = true;
        }

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
