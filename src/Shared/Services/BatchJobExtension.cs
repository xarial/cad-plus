using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public static class BatchJobExtension
    {
        public static void HandleExecute(this IBatchJob job, CancellationToken cancellationToken,
            Action<DateTime> raiseStartEventFunc, Action<DateTime> setStartTimeFunc,
            Action<CancellationToken> initFunc, Action raiseInitEventFunc,
            Action<CancellationToken> doWorkFunc, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<BatchJobStatus_e> setStatusFunc)
        {
            var startTime = DateTime.Now;

            setStartTimeFunc.Invoke(startTime);

            setStatusFunc.Invoke(BatchJobStatus_e.Initializing);

            raiseStartEventFunc.Invoke(startTime);

            try
            {
                initFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(BatchJobStatus_e.InProgress);

                raiseInitEventFunc.Invoke();

                doWorkFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Cancelled);
            }
            catch
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        public static async Task HandleExecuteAsync(this IAsyncBatchJob job, CancellationToken cancellationToken,
            Action<DateTime> raiseStartEventFunc, Action<DateTime> setStartTimeFunc,
            Func<CancellationToken, Task> initFunc, Action raiseInitEventFunc,
            Func<CancellationToken, Task> doWorkFunc, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<BatchJobStatus_e> setStatusFunc)
        {
            var startTime = DateTime.Now;

            setStartTimeFunc.Invoke(startTime);

            setStatusFunc.Invoke(BatchJobStatus_e.Initializing);

            raiseStartEventFunc.Invoke(startTime);

            try
            {
                await initFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(BatchJobStatus_e.InProgress);

                raiseInitEventFunc.Invoke();

                await doWorkFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Cancelled);
            }
            catch
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        private static BatchJobStatus_e ComposeJobStatus(IBatchJobBase job)
        {
            if (job.JobItems.All(i => i.State.Status == BatchJobItemStateStatus_e.Succeeded))
            {
                return BatchJobStatus_e.Succeeded;
            }
            else if (job.JobItems.Any(i => i.State.Status == BatchJobItemStateStatus_e.Succeeded))
            {
                return BatchJobStatus_e.CompletedWithWarning;
            }
            else
            {
                return BatchJobStatus_e.Failed;
            }
        }

    }
}
