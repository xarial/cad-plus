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
}
