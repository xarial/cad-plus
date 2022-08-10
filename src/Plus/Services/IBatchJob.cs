using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Xarial.CadPlus.Plus.Services
{
    public enum JobItemStateStatus_e
    {
        Initializing,
        InProgress,
        Failed,
        Succeeded,
        Warning
    }

    public enum JobStatus_e
    {
        InProgress,
        Failed,
        Succeeded,
        CompletedWithWarning,
        Cancelled
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

    public delegate void JobStateStatusChangedDelegate(IJobItemState sender, JobItemStateStatus_e status);
    public delegate void JobStateIssuesChangedDelegate(IJobItemState sender, IReadOnlyList<IJobItemIssue> issues);

    public delegate void JobItemOperationUserResultChangedDelegate(IJobItemOperation sender, object userResult);

    public delegate void JobInitializedDelegate(IBatchJobBase sender, IReadOnlyList<IJobItem> jobItems, IReadOnlyList<IJobItemOperationDefinition> operations, DateTime startTime);
    public delegate void JobCompletedDelegate(IBatchJobBase sender, TimeSpan duration);
    public delegate void JobItemProcessedDelegate(IBatchJobBase sender, IJobItem item, bool result);
    public delegate void JobProgressChangedDelegate(IBatchJobBase sender, double progress);
    public delegate void JobLogDelegateDelegate(IBatchJobBase sender, string message);

    public delegate void JobItemNestedItemsInitializedDelegate(IJobItem sender, IReadOnlyList<IJobItem> nestedItems);

    public interface IJobItem
    {
        event JobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        ImageSource Icon { get; }
        ImageSource Preview { get; }
        string Title { get; }
        string Description { get; }
        Action Link { get; }
        
        IJobItemState State { get; }

        IReadOnlyList<IJobItemOperation> Operations { get; }
        IReadOnlyList<IJobItem> Nested { get; }
    }

    public static class JobItemExtension 
    {
        public static JobItemStateStatus_e ComposeStatus(this IJobItem jobItem) 
        {
            var statuses = jobItem.Operations.Select(o => o.State.Status).ToArray();

            if (statuses.All(s => s == JobItemStateStatus_e.Initializing))
            {
                return JobItemStateStatus_e.Initializing;
            }
            else if (statuses.All(s => s == JobItemStateStatus_e.Succeeded))
            {
                return JobItemStateStatus_e.Succeeded;
            }
            else if (statuses.All(s => s == JobItemStateStatus_e.Failed))
            {
                return JobItemStateStatus_e.Failed;
            }
            else if (statuses.All(s => s == JobItemStateStatus_e.Failed || s == JobItemStateStatus_e.Succeeded))
            {
                return JobItemStateStatus_e.Warning;
            }
            else
            {
                return JobItemStateStatus_e.InProgress;
            }
        }
    }

    public interface IJobItemOperationDefinition 
    {
        string Name { get; }
        ImageSource Icon { get; }
    }

    public interface IJobItemOperation 
    {
        event JobItemOperationUserResultChangedDelegate UserResultChanged;
        IJobItemOperationDefinition Definition { get; }

        IJobItemState State { get; }
        object UserResult { get; }
    }

    public interface IJobItemState
    {
        event JobStateStatusChangedDelegate StatusChanged;
        event JobStateIssuesChangedDelegate IssuesChanged;

        JobItemStateStatus_e Status { get; }
        IReadOnlyList<IJobItemIssue> Issues { get; }
    }

    public interface IBatchJobBase : IDisposable
    {
        event JobInitializedDelegate Initialized;
        event JobCompletedDelegate Completed;
        event JobItemProcessedDelegate ItemProcessed;
        event JobLogDelegateDelegate Log;
        event JobProgressChangedDelegate ProgressChanged;

        IReadOnlyList<IJobItemOperationDefinition> OperationDefinitions { get; }
        IReadOnlyList<string> LogEntries { get; }
        IReadOnlyList<IJobItem> JobItems { get; }
    }

    public interface IBatchJob : IBatchJobBase
    {
        bool Execute(CancellationToken cancellationToken);
    }

    public interface IAsyncBatchJob : IBatchJobBase
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken);
    }
}
