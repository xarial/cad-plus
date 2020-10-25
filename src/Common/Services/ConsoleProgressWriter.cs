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

        public void ReportProgress(IJobItemFile file, bool result)
        {
            m_ProcessedFiles++;
            var prg = m_ProcessedFiles / (double)m_TotalFiles;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Progress: {(prg * 100).ToString("F")}%");
            Console.ResetColor();
        }

        public void SetJobScope(IJobItemFile[] scope, DateTime startTime)
        {
            m_TotalFiles = scope.Length;
            Console.WriteLine($"Processing {scope.Length} file(s)");
        }
        
        public void ReportCompleted(TimeSpan duration)
        {
        }
    }
}
