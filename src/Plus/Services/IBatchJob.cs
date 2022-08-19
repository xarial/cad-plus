using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xarial.CadPlus.Plus.Services
{
    public enum BatchJobItemStateStatus_e
    {
        Queued,
        InProgress,
        Failed,
        Succeeded,
        Warning
    }

    public enum BatchJobStatus_e
    {
        NotStarted,
        Initializing,
        InProgress,
        Failed,
        Succeeded,
        CompletedWithWarning,
        Cancelled
    }

    public enum BatchJobItemIssueType_e
    {
        Information,
        Warning,
        Error
    }

    public interface IBatchJobItemIssue 
    {
        BatchJobItemIssueType_e Type { get; }
        string Content { get; }
    }

    public interface IBatchJobState 
    {
        event BatchJobStateProgressChangedDelegate ProgressChanged;

        int TotalItemsCount { get; }
        int SucceededItemsCount { get; }
        int WarningItemsCount { get; }
        int FailedItemsCount { get; }

        DateTime StartTime { get; }
        TimeSpan Duration { get; }
        double Progress { get; }
        BatchJobStatus_e Status { get; }
    }

    public delegate void BatchJobStateStatusChangedDelegate(IBatchJobItemState sender, BatchJobItemStateStatus_e status);
    public delegate void BatchJobStateIssuesChangedDelegate(IBatchJobItemState sender, IReadOnlyList<IBatchJobItemIssue> issues);

    public delegate void BatchJobItemOperationUserResultChangedDelegate(IBatchJobItemOperation sender, object userResult);

    public delegate void BatchJobStartedDelegate(IBatchJobBase sender, DateTime startTime);
    public delegate void BatchJobInitializedDelegate(IBatchJobBase sender, IReadOnlyList<IBatchJobItem> jobItems, IReadOnlyList<IBatchJobItemOperationDefinition> operations);
    public delegate void BatchJobCompletedDelegate(IBatchJobBase sender, TimeSpan duration, BatchJobStatus_e status);
    public delegate void BatchJobItemProcessedDelegate(IBatchJobBase sender, IBatchJobItem item);
    public delegate void BatchJobStateProgressChangedDelegate(IBatchJobState sender, double progress);
    public delegate void BatchJobLogDelegateDelegate(IBatchJobBase sender, string message);

    public delegate void BatchJobItemNestedItemsInitializedDelegate(IBatchJobItem sender, IReadOnlyList<IBatchJobItem> nestedItems);

    public interface IBatchJobItem
    {
        event BatchJobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        BitmapImage Icon { get; }
        BitmapImage Preview { get; }
        string Title { get; }
        string Description { get; }
        Action Link { get; }
        
        IBatchJobItemState State { get; }

        IReadOnlyList<IBatchJobItemOperation> Operations { get; }
        IReadOnlyList<IBatchJobItem> Nested { get; }
    }

    public interface IBatchJobItemOperationDefinition 
    {
        string Name { get; }
        BitmapImage Icon { get; }
    }

    public interface IBatchJobItemOperation 
    {
        event BatchJobItemOperationUserResultChangedDelegate UserResultChanged;
        IBatchJobItemOperationDefinition Definition { get; }

        IBatchJobItemState State { get; }
        object UserResult { get; }
    }

    public interface IBatchJobItemState
    {
        event BatchJobStateStatusChangedDelegate StatusChanged;
        event BatchJobStateIssuesChangedDelegate IssuesChanged;

        BatchJobItemStateStatus_e Status { get; }
        IReadOnlyList<IBatchJobItemIssue> Issues { get; }
    }

    public interface IBatchJobBase : IDisposable
    {
        event BatchJobStartedDelegate Started;
        event BatchJobInitializedDelegate Initialized;
        event BatchJobCompletedDelegate Completed;
        event BatchJobItemProcessedDelegate ItemProcessed;
        event BatchJobLogDelegateDelegate Log;
        
        IBatchJobState State { get; }

        IReadOnlyList<IBatchJobItemOperationDefinition> OperationDefinitions { get; }
        IReadOnlyList<string> LogEntries { get; }
        IReadOnlyList<IBatchJobItem> JobItems { get; }
    }

    public interface IBatchJob : IBatchJobBase
    {
        void TryExecute(CancellationToken cancellationToken);
    }

    public interface IAsyncBatchJob : IBatchJobBase
    {
        Task TryExecuteAsync(CancellationToken cancellationToken);
    }
}
