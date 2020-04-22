//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.Xport.Core;

namespace Xarial.CadPlus.Xport
{
    public partial class App : Application
    {
        [DllImport("Kernel32.dll")]
        private static extern bool AttachConsole(int processId);

        private class ConsoleProgressWriter : IProgress<double>
        {
            public void Report(double value)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Progress: {(value * 100).ToString("F")}%");
                Console.ResetColor();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            if (e.Args.Any())
            {
                AttachConsole(-1);

                var parser = new Parser(p =>
                {
                    p.CaseInsensitiveEnumValues = true;
                    p.AutoHelp = true;
                    p.EnableDashDash = true;
                    p.HelpWriter = Console.Out;
                    p.IgnoreUnknownArguments = false;
                });

                var hasError = false;

                Arguments args = null;
                parser.ParseArguments<Arguments>(e.Args)
                    .WithParsed(a => args = a)
                    .WithNotParsed(err => hasError = true);

                var res = false;

                if (!hasError)
                {
                    try
                    {
                        RunConsoleExporter(args)
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

        private async Task RunConsoleExporter(Arguments args)
        {
            var opts = new ExportOptions()
            {
                Input = args.Input?.ToArray(),
                Filter = args.Filter,
                Format = args.Format?.ToArray(),
                Timeout = args.Timeout,
                OutputDirectory = args.OutputDirectory,
                ContinueOnError = args.ContinueOnError
            };

            using (var exporter = new Exporter(Console.Out, new ConsoleProgressWriter()))
            {
                await exporter.Export(opts).ConfigureAwait(false);
            }
        }

        private void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}