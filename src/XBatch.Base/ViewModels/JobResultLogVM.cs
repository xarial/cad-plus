using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultLogVM : IJobResultLog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_Output;

        public string Output
        {
            get => m_Output;
            set 
            {
                m_Output = value;
                this.NotifyChanged();
            }
        }
    }
}
