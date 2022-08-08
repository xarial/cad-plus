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
    public class ConsoleProgressWriter : IDisposable
    {
        private IReadOnlyList<IJobItem> m_Scope;

        private readonly IBatchJobBase m_Job;

        public ConsoleProgressWriter(IBatchJobBase job) 
        {
            m_Job = job;
            m_Job.JobCompleted += OnJobCompleted;
            m_Job.JobSet += OnJobSet;
            m_Job.ItemProcessed += OnProgressChanged;
            m_Job.Log += OnLog;
        }

        private void OnJobCompleted(IBatchJobBase sender, TimeSpan duration) => ReportCompleted(duration);
        private void OnJobSet(IBatchJobBase sender, IReadOnlyList<IJobItem> jobItems, IReadOnlyList<IJobItemOperationDefinition> operations, DateTime startTime) => SetJobScope(jobItems, startTime);
        private void OnProgressChanged(IBatchJobBase sender, IJobItem file, double progress, bool result) => ReportProgress(file, progress, result);
        private void OnLog(IBatchJobBase sender, string msg) => Log(msg);

        private void Log(string msg) => Console.WriteLine(msg);

        private void ReportProgress(IJobItem file, double progress, bool result)
        {
            Console.WriteLine($"Result: {file.State.Status}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Progress: {(progress * 100).ToString("F")}%");
            Console.ResetColor();
        }

        private void SetJobScope(IReadOnlyList<IJobItem> scope, DateTime startTime)
        {
            m_Scope = scope;
            Console.WriteLine($"Processing {scope.Count} file(s). {startTime}");
        }

        private void ReportCompleted(TimeSpan duration)
        {
            Console.WriteLine($"Operation completed: {duration}");
            Console.WriteLine($"Processed: {m_Scope.Count(j => j.State.Status == JobItemStateStatus_e.Succeeded)}");
            Console.WriteLine($"Warning: {m_Scope.Count(j => j.State.Status == JobItemStateStatus_e.Warning)}");
            Console.WriteLine($"Failed: {m_Scope.Count(j => j.State.Status == JobItemStateStatus_e.Failed)}");
        }

        public void Dispose()
        {
            m_Job.JobCompleted -= OnJobCompleted;
            m_Job.JobSet -= OnJobSet;
            m_Job.ItemProcessed -= OnProgressChanged;
            m_Job.Log -= OnLog;
        }
    }
}
