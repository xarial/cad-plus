using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
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

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public JobsManagerVM(IBatchRunnerModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;
            
            JobDocuments = new ObservableCollection<JobDocumentVM>();

            NewDocumentCommand = new RelayCommand(OnNewDocument);
        }

        private void OnNewDocument() 
        {
            //TODO: create unique name
            JobDocuments.Add(new JobDocumentVM("New xBatch Job Document", new BatchJob(), m_Model));
        }
    }
}
