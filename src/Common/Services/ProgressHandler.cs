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

    public interface IProgressHandler : IProgress<double>
    {
        void SetJobScope(IEnumerable<IJobItemFile> scope);
    }

    public class ProgressHandler : IProgressHandler
    {
        public event Action<double> ProgressChanged;
        public event Action<IEnumerable<IJobItemFile>> JobScopeSet;

        public void Report(double value)
        {
            ProgressChanged?.Invoke(value);
        }

        public void SetJobScope(IEnumerable<IJobItemFile> scope)
        {
            JobScopeSet?.Invoke(scope);
        }
    }
}
