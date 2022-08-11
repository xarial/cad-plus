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
        Initializing,
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
    public delegate void JobCompletedDelegate(IBatchJobBase sender, TimeSpan duration, JobStatus_e status);
    public delegate void JobItemProcessedDelegate(IBatchJobBase sender, IJobItem item);
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

        DateTime StartTime { get; }
        TimeSpan Duration { get; }

        double Progress { get; }
        JobStatus_e Status { get; }

        IReadOnlyList<IJobItemOperationDefinition> OperationDefinitions { get; }
        IReadOnlyList<string> LogEntries { get; }
        IReadOnlyList<IJobItem> JobItems { get; }
    }

    public interface IBatchJob : IBatchJobBase
    {
        void TryExecute(CancellationToken cancellationToken);
    }

    public interface IAsyncBatchJob : IBatchJobBase
    {
        Task TryExecuteAsync(CancellationToken cancellationToken);
    }

    public static class BatchJobExtension
    {
        public static void HandleExecute(this IBatchJob job, CancellationToken cancellationToken,
            Action<CancellationToken> initFunc, Action<DateTime> raiseInitEventFunc, Action<DateTime> setStartTime,
            Action<CancellationToken> doWorkFunc, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<JobStatus_e> setStatusFunc)
        {
            var startTime = DateTime.Now;

            try
            {
                initFunc.Invoke(cancellationToken);

                setStartTime.Invoke(startTime);

                raiseInitEventFunc.Invoke(startTime);

                setStatusFunc.Invoke(JobStatus_e.InProgress);

                doWorkFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(JobStatus_e.Cancelled);
            }
            catch
            {
                setStatusFunc.Invoke(JobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        public static async Task HandleExecuteAsync(this IAsyncBatchJob job, CancellationToken cancellationToken,
            Func<CancellationToken, Task> initFunc, Action<DateTime> raiseInitEventFunc, Action<DateTime> setStartTime,
            Func<CancellationToken, Task> doWorkFunc, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<JobStatus_e> setStatusFunc)
        {
            var startTime = DateTime.Now;

            try
            {
                await initFunc.Invoke(cancellationToken);
                
                setStartTime.Invoke(startTime);

                raiseInitEventFunc.Invoke(startTime);

                setStatusFunc.Invoke(JobStatus_e.InProgress);

                await doWorkFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(JobStatus_e.Cancelled);
            }
            catch
            {
                setStatusFunc.Invoke(JobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        private static JobStatus_e ComposeJobStatus(IBatchJobBase job)
        {
            if (job.JobItems.All(i => i.State.Status == JobItemStateStatus_e.Succeeded))
            {
                return JobStatus_e.Succeeded;
            }
            else if (job.JobItems.Any(i => i.State.Status == JobItemStateStatus_e.Succeeded))
            {
                return JobStatus_e.CompletedWithWarning;
            }
            else
            {
                return JobStatus_e.Failed;
            }
        }
    }
}
