using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultsVM : IJobResults, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IJobResult m_Selected;

        public IJobResult Selected 
        {
            get => m_Selected;
            set 
            {
                m_Selected = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<JobResultVM> Items { get; }

        public JobResultsVM() 
        {
            Items = new ObservableCollection<JobResultVM>();
        }
    }
}
