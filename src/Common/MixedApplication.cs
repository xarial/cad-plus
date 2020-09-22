using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Common
{
    internal static class WindowsApi 
    {
        [DllImport("Kernel32.dll")]
        internal static extern bool AttachConsole(int processId);
    }

    public abstract class MixedApplication<TArgs> : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            if (e.Args.Any())
            {
                WindowsApi.AttachConsole(-1);

                Console.WriteLine("Hello");

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
                parser.ParseArguments<TArgs>(e.Args)
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
                base.OnStartup(e);
            }
        }

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
