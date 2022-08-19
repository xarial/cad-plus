using System;
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
        public static void ProcessJobItem<T>(T batchJobItem, BatchJobItemState jobItemState, BatchJobState jobState,
            double progress, CancellationToken cancellationToken, Action<T, CancellationToken> runJobitemFunc,
            Action<T> raiseItemProcessedEvent)
            where T : IBatchJobItem
        {
            if (jobItemState.Status == BatchJobItemStateStatus_e.Queued)
            {
                ProcessAction(jobItemState, () =>
                {
                    runJobitemFunc.Invoke(batchJobItem, cancellationToken);
                    return batchJobItem.ComposeStatus();
                });

                switch (jobItemState.Status)
                {
                    case BatchJobItemStateStatus_e.Succeeded:
                        jobState.SucceededItemsCount++;
                        break;

                    case BatchJobItemStateStatus_e.Warning:
                        jobState.WarningItemsCount++;
                        break;

                    case BatchJobItemStateStatus_e.Failed:
                        jobState.FailedItemsCount++;
                        break;
                }
            }

            jobState.Progress = progress;
            raiseItemProcessedEvent?.Invoke(batchJobItem);
        }

        public static void ProcessAction(BatchJobItemState state, Func<BatchJobItemStateStatus_e> action) 
        {
            try
            {
                state.Status = BatchJobItemStateStatus_e.InProgress;

                state.Status = action.Invoke();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                state.ReportError(ex);
            }
        }
    }
}
