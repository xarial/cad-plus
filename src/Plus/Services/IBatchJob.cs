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
    public enum JobItemState_e
    {
        Initializing,
        InProgress,
        Failed,
        Succeeded,
        Warning
    }

    public enum JobState_e
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

    public delegate void JobItemOperationStateChangedDelegate(IJobItemOperation sender, JobItemState_e state);
    public delegate void JobItemOperationIssuesChangedDelegate(IJobItemOperation sender, IReadOnlyList<IJobItemIssue> issues);
    public delegate void JobItemOperationUserResultChangedDelegate(IJobItemOperation sender, object userResult);

    public delegate void JobSetDelegate(IBatchJobBase sender, IReadOnlyList<IJobItem> jobItems, IReadOnlyList<IJobItemOperationDefinition> operations, DateTime startTime);
    public delegate void JobCompletedDelegate(IBatchJobBase sender, TimeSpan duration);
    public delegate void JobItemProcessedDelegate(IBatchJobBase sender, IJobItem item, double progress, bool result);
    public delegate void JobLogDelegateDelegate(IBatchJobBase sender, string message);

    public interface IJobItem
    {
        ImageSource Icon { get; }
        ImageSource Preview { get; }
        string Title { get; }
        string Description { get; }
        Action Link { get; }
        
        IReadOnlyList<IJobItemOperation> Operations { get; }
        IReadOnlyList<IJobItem> Nested { get; }
    }

    public interface IJobItemOperationDefinition 
    {
        string Name { get; }
        ImageSource Icon { get; }
    }

    public interface IJobItemOperation 
    {
        event JobItemOperationStateChangedDelegate StateChanged;
        event JobItemOperationIssuesChangedDelegate IssuesChanged;
        event JobItemOperationUserResultChangedDelegate UserResultChanged;

        IJobItemOperationDefinition Definition { get; }

        JobItemState_e State { get; }
        IReadOnlyList<IJobItemIssue> Issues { get; }
        object UserResult { get; }
    }

    public interface IBatchJobBase : IDisposable
    {
        event JobSetDelegate JobSet;
        event JobCompletedDelegate JobCompleted;
        event JobItemProcessedDelegate ItemProcessed;
        event JobLogDelegateDelegate Log;

        IJobItem[] JobItems { get; }
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
