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
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Common.Services
{
    public class ConsoleProgressWriter
    {
        private IReadOnlyList<IJobItem> m_Scope;

        public void Log(string msg) => Console.WriteLine(msg);

        public void ReportProgress(IJobItem file, double progress, bool result)
        {
            Console.WriteLine($"Result: {file.ResolveState()}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Progress: {(progress * 100).ToString("F")}%");
            Console.ResetColor();
        }

        public void SetJobScope(IReadOnlyList<IJobItem> scope, DateTime startTime)
        {
            m_Scope = scope;
            Console.WriteLine($"Processing {scope.Count} file(s). {startTime}");
        }
        
        public void ReportCompleted(TimeSpan duration)
        {
            Console.WriteLine($"Operation completed: {duration}");
            Console.WriteLine($"Processed: {m_Scope.Count(j => j.ResolveState() == JobItemState_e.Succeeded)}");
            Console.WriteLine($"Warning: {m_Scope.Count(j => j.ResolveState() == JobItemState_e.Warning)}");
            Console.WriteLine($"Failed: {m_Scope.Count(j => j.ResolveState() == JobItemState_e.Failed)}");
        }

        public void Dispose()
        {
        }
    }
}
