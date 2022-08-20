using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchJobState : IBatchJobState
    {
        public event BatchJobStateProgressChangedDelegate ProgressChanged;

        public int TotalItemsCount { get; set; }
        public int SucceededItemsCount { get; set; }
        public int WarningItemsCount { get; set; }
        public int FailedItemsCount { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public BatchJobStatus_e Status { get; set; }

        public double Progress
        {
            get => m_Progress;
            set
            {
                Duration = DateTime.Now.Subtract(StartTime);
                m_Progress = value;
                this.ProgressChanged?.Invoke(this, value);
            }
        }

        private double m_Progress;
    }

    public static class BatchJobStateExtension 
    {
        public static void IncrementItemsCount(this BatchJobState jobState, IBatchJobItem batchJobItem) 
        {
            var status = batchJobItem.State.Status;

            switch (status)
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

                default:
                    throw new Exception($"Job item '{batchJobItem.Title}' is not in the completed state: '{status}'");
            }
        }
    }
}
