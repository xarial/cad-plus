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
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public static class BatchJobHelper
    {
        public static void HandleJobExecute(this IBatchJob job,
            Action<DateTime> raiseStartEventFunc, Action<DateTime> setStartTimeFunc,
            Func<CancellationToken, int> initFunc, Action<int> setTotalItemsFunc, Action raiseInitEventFunc,
            Action<CancellationToken> doWorkFunc, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<BatchJobStatus_e> setStatusFunc, CancellationToken cancellationToken, IXLogger logger)
        {
            var startTime = DateTime.Now;

            setStartTimeFunc.Invoke(startTime);

            setStatusFunc.Invoke(BatchJobStatus_e.Initializing);

            raiseStartEventFunc.Invoke(startTime);

            try
            {
                logger.Log("Initiating a job", LoggerMessageSeverity_e.Debug);

                var totalItems = initFunc.Invoke(cancellationToken);

                setTotalItemsFunc.Invoke(totalItems);

                setStatusFunc.Invoke(BatchJobStatus_e.InProgress);

                raiseInitEventFunc.Invoke();

                logger.Log("Performing a job work", LoggerMessageSeverity_e.Debug);

                doWorkFunc.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Cancelled);
            }
            catch(Exception ex)
            {
                logger.Log(ex);

                setStatusFunc.Invoke(BatchJobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        public static async Task HandleJobExecuteAsync(this IAsyncBatchJob job,
            Action<DateTime> raiseStartEventFunc, Action<DateTime> setStartTimeFunc,
            Func<CancellationToken, Task<int>> initFuncAsync, Action<int> setTotalItemsFunc, Action raiseInitEventFunc,
            Func<CancellationToken, Task> doWorkFuncAsync, Action<TimeSpan> raiseCompletedFunc, Action<TimeSpan> setDuration,
            Action<BatchJobStatus_e> setStatusFunc, CancellationToken cancellationToken, IXLogger logger)
        {
            var startTime = DateTime.Now;

            setStartTimeFunc.Invoke(startTime);

            setStatusFunc.Invoke(BatchJobStatus_e.Initializing);

            raiseStartEventFunc.Invoke(startTime);

            try
            {
                logger.Log("Initiating a job", LoggerMessageSeverity_e.Debug);

                var totalItems = await initFuncAsync.Invoke(cancellationToken);

                setTotalItemsFunc.Invoke(totalItems);

                setStatusFunc.Invoke(BatchJobStatus_e.InProgress);

                raiseInitEventFunc.Invoke();

                logger.Log("Performing a job work", LoggerMessageSeverity_e.Debug);

                await doWorkFuncAsync.Invoke(cancellationToken);

                setStatusFunc.Invoke(ComposeJobStatus(job));
            }
            catch (OperationCanceledException)
            {
                setStatusFunc.Invoke(BatchJobStatus_e.Cancelled);
            }
            catch(Exception ex)
            {
                logger.Log(ex);

                setStatusFunc.Invoke(BatchJobStatus_e.Failed);
            }
            finally
            {
                var duration = DateTime.Now.Subtract(startTime);
                setDuration.Invoke(duration);
                raiseCompletedFunc?.Invoke(duration);
            }
        }

        public static void HandleJobItem<T>(this T batchJobItem,
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

                    if (batchJobItem.State.Status == BatchJobItemStateStatus_e.InProgress)
                    {
                        setStatusFunc.Invoke(batchJobItem, batchJobItem.ComposeStatus());
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    setErrorFunc.Invoke(batchJobItem, ex);

                    if (batchJobItem.State.Status != BatchJobItemStateStatus_e.Failed)
                    {
                        setStatusFunc.Invoke(batchJobItem, BatchJobItemStateStatus_e.Failed);
                    }
                }

                incrementStateItemsFunc.Invoke(batchJobItem);
            }

            updateProgressFunc.Invoke();
            raiseItemProcessedEventFunc.Invoke(batchJobItem);
        }

        public static async Task HandleJobItemAsync<T>(this T batchJobItem,
            Action<T, BatchJobItemStateStatus_e> setStatusFunc, Func<T, Task> runJobItemAsyncFunc, Action<T, Exception> setErrorFunc,
            Action<T> incrementStateItemsFunc,
            Action updateProgressFunc, Action<T> raiseItemProcessedEventFunc)
            where T : IBatchJobItem
        {
            if (batchJobItem.State.Status == BatchJobItemStateStatus_e.Queued)
            {
                try
                {
                    setStatusFunc.Invoke(batchJobItem, BatchJobItemStateStatus_e.InProgress);

                    await runJobItemAsyncFunc.Invoke(batchJobItem);

                    if (batchJobItem.State.Status == BatchJobItemStateStatus_e.InProgress)
                    {
                        setStatusFunc.Invoke(batchJobItem, batchJobItem.ComposeStatus());
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    setErrorFunc.Invoke(batchJobItem, ex);

                    if (batchJobItem.State.Status != BatchJobItemStateStatus_e.Failed)
                    {
                        setStatusFunc.Invoke(batchJobItem, BatchJobItemStateStatus_e.Failed);
                    }
                }

                incrementStateItemsFunc.Invoke(batchJobItem);
            }

            updateProgressFunc.Invoke();
            raiseItemProcessedEventFunc.Invoke(batchJobItem);
        }

        public static void HandleJobItemOperation<T>(this T oper,
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

        public static async Task HandleJobItemOperationAsync<T>(this T oper,
            Action<T, BatchJobItemStateStatus_e> setStatusFunc, Func<T, Task> runJobItemAsyncFunc, Action<T, Exception> setErrorFunc)
            where T : IBatchJobItemOperation
        {
            try
            {
                setStatusFunc.Invoke(oper, BatchJobItemStateStatus_e.InProgress);
                await runJobItemAsyncFunc.Invoke(oper);
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
