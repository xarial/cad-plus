using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

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
        public ICommand OpenDocumentCommand { get; }
        public ICommand SaveAllDocumentsCommand { get; }

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public JobsManagerVM(IBatchRunnerModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;
            
            JobDocuments = new ObservableCollection<JobDocumentVM>();

            NewDocumentCommand = new RelayCommand(OnNewDocument);
            OpenDocumentCommand = new RelayCommand(OpenDocument);
            SaveAllDocumentsCommand = new RelayCommand(SaveAllDocuments, () => JobDocuments.Any(d => d.IsDirty));
        }

        private void SaveAllDocuments()
        {
            foreach (var dirtyDoc in JobDocuments.Where(d => d.IsDirty)) 
            {
                dirtyDoc.SaveDocument();
            }
        }

        private void OpenDocument()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileOpen(out string filePath, "Select file to open",
                    FileSystemBrowser.BuildFilterString(new FileFilter("xBatch File", "*.xbatch"), FileFilter.AllFiles))) 
                {
                    if (!JobDocuments.Any(d => string.Equals(d.FilePath, filePath, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        var svc = new UserSettingsService();

                        var batchJob = svc.ReadSettings<BatchJob>(filePath);

                        JobDocuments.Add(new JobDocumentVM(new FileInfo(filePath), batchJob, m_Model, m_MsgSvc));
                    }
                    else 
                    {
                        m_MsgSvc.ShowError("Document already open");
                    }
                }
            }
            catch 
            {
                m_MsgSvc.ShowError("Failed to open document");
            }
        }

        private void OnNewDocument() 
        {
            int index = 0;

            var name = "";

            do
            {
                name = $"xBatch Document {++index}";
            }
            while (JobDocuments.Any(d => string.Equals(d.Name, name, StringComparison.CurrentCultureIgnoreCase)));

            JobDocuments.Add(new JobDocumentVM(name, new BatchJob(), m_Model, m_MsgSvc));
        }
    }
}
