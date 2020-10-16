using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobsManagerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<JobDocumentVM> JobDocuments { get; }

        public JobDocumentVM ActiveJob 
        {
            get => m_ActiveJob;
            set 
            {
                m_ActiveJob = value;
                this.NotifyChanged();
            }
        }

        private JobDocumentVM m_ActiveJob;

        public ICommand NewDocumentCommand { get; }

        public JobsManagerVM()
        {
            JobDocuments = new ObservableCollection<JobDocumentVM>();

            NewDocumentCommand = new RelayCommand(OnNewDocument);
        }

        private void OnNewDocument() 
        {
            //TODO: create unique name
            JobDocuments.Add(new JobDocumentVM("New xBatch Job Document"));
        }
    }
}
