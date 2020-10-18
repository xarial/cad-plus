using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultSummaryVM : INotifyPropertyChanged
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

        public bool IsInitializing 
        {
            get => m_IsInitializing;
            set 
            {
                if (value != m_IsInitializing)
                {
                    m_IsInitializing = value;
                    this.NotifyChanged();
                }
            }
        }

        private JobItemFileVM[] m_JobItemFiles;

        public JobItemFileVM[] JobItemFiles 
        {
            get => m_JobItemFiles;
            set 
            {
                m_JobItemFiles = value;
                this.NotifyChanged();
            }
        }

        private readonly IBatchRunJobExecutor m_Executor;
        private double m_Progress;
        private bool m_IsInitializing;

        public JobResultSummaryVM(IBatchRunJobExecutor executor)
        {
            m_Executor = executor;

            m_Executor.JobSet += OnJobSet;
            m_Executor.ProgressChanged += OnProgressChanged;
        }

        private void OnJobSet(IEnumerable<IJobItemFile> files)
        {
            JobItemFiles = files.Select(f => new JobItemFileVM(f)).ToArray();
        }

        private void OnProgressChanged(double prg)
        {
            if (double.IsNaN(prg))
            {
                IsInitializing = true;
            }
            else
            {
                IsInitializing = false;
                Progress = prg;
            }
        }
    }
}
