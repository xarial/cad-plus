using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobSettingsVM : IJobSettings, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool ContinueOnError
        {
            get => m_Job.ContinueOnError;
            set
            {
                m_Job.ContinueOnError = value;
                this.NotifyChanged();
            }
        }

        public int Timeout
        {
            get => m_Job.Timeout;
            set
            {
                m_CachedTimeout = value;
                m_Job.Timeout = value;
                this.NotifyChanged();
            }
        }

        public bool IsTimeoutEnabled
        {
            get => m_Job.Timeout != -1;
            set
            {
                if (!value)
                {
                    m_Job.Timeout = -1;
                }
                else
                {
                    m_Job.Timeout = m_CachedTimeout;
                }

                this.NotifyChanged();
            }
        }

        public StartupOptions_e StartupOptions
        {
            get => m_Job.StartupOptions;
            set
            {
                m_Job.StartupOptions = value;
                this.NotifyChanged();
            }
        }

        public OpenFileOptions_e OpenFileOptions
        {
            get => m_Job.OpenFileOptions;
            set
            {
                m_Job.OpenFileOptions = value;
                this.NotifyChanged();
            }
        }

        private int m_CachedTimeout;
        private readonly BatchJob m_Job;

        public JobSettingsVM(BatchJob job) 
        {
            m_Job = job;
            m_CachedTimeout = m_Job.Timeout;
        }
    }
}
