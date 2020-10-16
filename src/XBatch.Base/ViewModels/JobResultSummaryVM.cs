using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultSummaryVM : IJobResultSummary, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        private readonly IBatchRunJobExecutor m_Executor;
        private double m_Progress;

        public JobResultSummaryVM(IBatchRunJobExecutor executor)
        {
            m_Executor = executor;

            m_Executor.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(double prg)
        {
            Progress = prg;
        }
    }
}
