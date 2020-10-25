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
    public enum JobItemStatus_e
    {
        AwaitingProcessing,
        InProgress,
        Failed,
        Succeeded,
        Warning
    }

    public interface IJobItemFile : IJobItem
    {
        IEnumerable<IJobItemOperation> Operations { get; }
    }

    public interface IJobItemOperation : IJobItem
    {
    }

    public interface IJobItem
    {
        event Action<IJobItem, JobItemStatus_e> StatusChanged;
        JobItemStatus_e Status { get; }
        string DisplayName { get; }
    }

    public interface IProgressHandler
    {
        void ReportProgress(IJobItemFile file, bool result);
        void SetJobScope(IJobItemFile[] scope, DateTime startTime);
        void ReportCompleted(TimeSpan duration);
    }

    public class ProgressHandler : IProgressHandler
    {
        public event Action<IJobItemFile, bool> ProgressChanged;
        public event Action<IJobItemFile[], DateTime> JobScopeSet;
        public event Action<TimeSpan> Completed;

        public void ReportCompleted(TimeSpan duration) => Completed?.Invoke(duration);
        public void ReportProgress(IJobItemFile file, bool result) => ProgressChanged?.Invoke(file, result);
        public void SetJobScope(IJobItemFile[] scope, DateTime startTime) => JobScopeSet?.Invoke(scope, startTime);
    }
}
