//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Services
{
    public class ConsoleProgressWriter : IProgressHandler
    {
        private int m_TotalFiles;
        private int m_ProcessedFiles;
        private IJobItem[] m_Scope;

        public void ReportProgress(IJobItem file, bool result)
        {
            m_ProcessedFiles++;
            var prg = m_ProcessedFiles / (double)m_TotalFiles;

            Console.WriteLine($"Result: {file.Status}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Progress: {(prg * 100).ToString("F")}%");
            Console.ResetColor();
        }

        public void SetJobScope(IJobItem[] scope, DateTime startTime)
        {
            m_Scope = scope;
            m_TotalFiles = scope.Length;
            Console.WriteLine($"Processing {scope.Length} file(s). {startTime}");
        }
        
        public void ReportCompleted(TimeSpan duration)
        {
            Console.WriteLine($"Operation completed: {duration}");
            Console.WriteLine($"Processed: {m_Scope.Count(j => j.Status == JobItemStatus_e.Succeeded)}");
            Console.WriteLine($"Warning: {m_Scope.Count(j => j.Status == JobItemStatus_e.Warning)}");
            Console.WriteLine($"Failed: {m_Scope.Count(j => j.Status == JobItemStatus_e.Failed)}");
        }
    }
}
