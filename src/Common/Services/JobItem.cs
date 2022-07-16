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

    public interface IJobItemDocument : IJobItem
    {
        IEnumerable<IJobItemOperation> Operations { get; }
    }

    public interface IJobItemOperation : IJobItem
    {
    }

    public interface IJobItem
    {
        event Action<IJobItem, JobItemStatus_e> StatusChanged;
        event Action<IJobItem> IssuesChanged;

        IReadOnlyList<string> Issues { get; }
        JobItemStatus_e Status { get; }
        string DisplayName { get; }
    }
}
