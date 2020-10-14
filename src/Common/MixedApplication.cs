//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Navigation;
using System.Windows.Threading;
using Xarial.CadPlus.Module.Init;

namespace Xarial.CadPlus.Common
{
    internal static class WindowsApi 
    {
        [DllImport("Kernel32.dll")]
        internal static extern bool AttachConsole(int processId);
    }

    public class ConsoleHostModule : BaseHostModule
    {
        public override IntPtr ParentWindow => IntPtr.Zero;

        public override event Action Loaded;

        internal ConsoleHostModule() 
        {
            Loaded?.Invoke();
        }
    }

    public class WpfAppHostModule : BaseHostModule
    {
        private readonly Application m_App;

        internal WpfAppHostModule(Application app)
        {
            m_App = app;
            m_App.Activated += OnAppActivated;
        }

        public override IntPtr ParentWindow => m_App.MainWindow != null
            ? new WindowInteropHelper(m_App.MainWindow).Handle
            : IntPtr.Zero;

        public override event Action Loaded;

        private void OnAppActivated(object sender, EventArgs e)
        {
            m_App.Activated -= OnAppActivated;
            Loaded?.Invoke();
        }
    }

    public abstract class MixedApplication<TArgs> : Application
    {
        private BaseHostModule m_HostModule;

        protected virtual void OnAppStart()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            OnAppStart();

            if (e.Args.Any())
            {
                WindowsApi.AttachConsole(-1);

                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                m_HostModule = new ConsoleHostModule();

                var parser = new Parser(p =>
                {
                    p.CaseInsensitiveEnumValues = true;
                    p.AutoHelp = true;
                    p.EnableDashDash = true;
                    p.HelpWriter = Console.Out;
                    p.IgnoreUnknownArguments = false;
                });

                var hasError = false;

                TArgs args = default;
                parser.ParseArguments<TArgs>(CreateArguments, e.Args)
                    .WithParsed(a => args = a)
                    .WithNotParsed(err => hasError = true);

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

                Environment.Exit(res ? 0 : 1);
            }
            else
            {
                m_HostModule = new WpfAppHostModule(this);
                base.OnStartup(e);
            }
        }
                
        protected virtual TArgs CreateArguments() => (TArgs)Activator.CreateInstance(typeof(TArgs));

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
        }

        protected abstract Task RunConsole(TArgs args);

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
