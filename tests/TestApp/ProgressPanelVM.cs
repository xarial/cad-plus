using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Extensions;

namespace TestApp
{
    public class ProgressPanelVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsWorkInProgress;
        private double? m_Progress;
        private string m_ProgressMessage;

        public bool IsWorkInProgress
        {
            get => m_IsWorkInProgress;
            set
            {
                m_IsWorkInProgress = value;
                this.NotifyChanged();
            }
        }

        public string ProgressMessage
        {
            get => m_ProgressMessage;
            set
            {
                m_ProgressMessage = value;
                this.NotifyChanged();
            }
        }

        public double? Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }
    }
}
