using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public enum JobItemStatus_e
    {
        AwaitingProcessing,
        InProgress,
        Failed,
        Succeeded,
        Warning
    }

    public enum IssueType_e
    {
        Information,
        Warning,
        Error
    }

    public interface IJobItemIssue 
    {
        IssueType_e Type { get; }
        string Content { get; }
    }

    public delegate void JobItemStatusChangedDelegate(IJobItem sender, JobItemStatus_e status);
    public delegate void JobItemIssuesChanged(IJobItem sender, IReadOnlyList<IJobItemIssue> issues);

    public delegate void JobSetDelegate(IBatchJobExecutorBase sender, IJobItem[] jobItems, DateTime startTime);
    public delegate void JobCompletedDelegate(IBatchJobExecutorBase sender, TimeSpan duration);
    public delegate void JobStatusChangedDelegate(IBatchJobExecutorBase sender, string status);
    public delegate void JobItemProcessedDelegate(IBatchJobExecutorBase sender, IJobItem item, double progress, bool result);
    public delegate void JobLogDelegateDelegate(IBatchJobExecutorBase sender, string message);

    public interface IJobItem
    {
        event JobItemStatusChangedDelegate StatusChanged;
        event JobItemIssuesChanged IssuesChanged;

        IReadOnlyList<IJobItemIssue> Issues { get; }
        JobItemStatus_e Status { get; }
        string DisplayName { get; }
    }

    public interface IBatchJobExecutorBase : IDisposable
    {
        event JobSetDelegate JobSet;
        event JobCompletedDelegate JobCompleted;
        event JobStatusChangedDelegate StatusChanged;
        event JobItemProcessedDelegate ItemProcessed;
        event JobLogDelegateDelegate Log;
    }

    public interface IBatchJobExecutor : IBatchJobExecutorBase
    {
        bool Execute(CancellationToken cancellationToken);
    }

    public interface IAsyncBatchJobExecutor : IBatchJobExecutorBase
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken);
    }
}
