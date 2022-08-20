﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public static class BatchJobHelper
    {
        public static void HandleJobExecute(this IBatchJob job, CancellationToken cancellationToken,
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

        public static async Task HandleJobExecuteAsync(this IAsyncBatchJob job, CancellationToken cancellationToken,
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

        public static void ProcessJobItem<T>(this T batchJobItem,
            Action<T, BatchJobItemStateStatus_e> setStatusFunc, Action<T> runJobItemFunc, Action<T, Exception> setErrorFunc,
            Action<T> incrementStateItemsFunc,
            Action updateProgressFunc, Action<T> raiseItemProcessedEventFunc)
            where T : IBatchJobItem
        {
            if (batchJobItem.State.Status == BatchJobItemStateStatus_e.Queued)
            {
                try
                {
                    setStatusFunc.Invoke(batchJobItem, BatchJobItemStateStatus_e.InProgress);
                    
                    runJobItemFunc.Invoke(batchJobItem);

                    setStatusFunc.Invoke(batchJobItem, batchJobItem.ComposeStatus());
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    setErrorFunc.Invoke(batchJobItem, ex);

                    setStatusFunc.Invoke(batchJobItem, BatchJobItemStateStatus_e.Failed);
                }

                incrementStateItemsFunc.Invoke(batchJobItem);
            }

            updateProgressFunc.Invoke();
            raiseItemProcessedEventFunc.Invoke(batchJobItem);
        }

        public static void ProcessJobItemOperation<T>(this T oper,
            Action<T, BatchJobItemStateStatus_e> setStatusFunc, Action<T> runJobItemFunc, Action<T, Exception> setErrorFunc)
            where T : IBatchJobItemOperation
        {
            try
            {
                setStatusFunc.Invoke(oper, BatchJobItemStateStatus_e.InProgress);
                runJobItemFunc.Invoke(oper);
                setStatusFunc.Invoke(oper, BatchJobItemStateStatus_e.Succeeded);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                setErrorFunc.Invoke(oper, ex);
                setStatusFunc.Invoke(oper, BatchJobItemStateStatus_e.Failed);
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
