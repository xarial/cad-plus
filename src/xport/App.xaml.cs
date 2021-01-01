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
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Xport.Core;

namespace Xarial.CadPlus.Xport
{
    public class ExportApplication : IApplication
    {
        public Guid Id => Guid.Parse("EA027A58-D1AF-4D3F-840D-1A11BD23A182");
    }

    public class XPortApp : MixedApplication<Arguments>
    {
        public XPortApp() : base(new ExportApplication()) 
        {
        }

        protected override Task RunConsole(Arguments args)
        {
            return RunConsoleExporter(args);
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