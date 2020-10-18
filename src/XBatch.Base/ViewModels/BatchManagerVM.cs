//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
    public class BatchManagerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BatchDocumentVM Document
        {
            get => m_Document;
            set 
            {
                m_Document = value;
                this.NotifyChanged();
            }
        }

        private BatchDocumentVM m_Document;

        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand CloseDocumentCommand { get; }

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public BatchManagerVM(IBatchRunnerModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;
            
            NewDocumentCommand = new RelayCommand(NewDocument);
            OpenDocumentCommand = new RelayCommand(OpenDocument);
            CloseDocumentCommand = new RelayCommand(CloseDocument, () => Document != null);
        }
        
        private void OpenDocument()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileOpen(out string filePath, "Select file to open",
                    FileSystemBrowser.BuildFilterString(new FileFilter("xBatch File", "*.xbatch"), FileFilter.AllFiles))) 
                {
                    if (!string.Equals(Document?.FilePath, filePath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var svc = new UserSettingsService();

                        var batchJob = svc.ReadSettings<BatchJob>(filePath);

                        if (Document == null)
                        {
                            Document = new BatchDocumentVM(new FileInfo(filePath), batchJob, m_Model, m_MsgSvc);
                        }
                        else 
                        {
                            //TODO: open in current session or in new session
                        }
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

        private void NewDocument() 
        {
            if (Document == null)
            {
                Document = new BatchDocumentVM("xBatch Document", new BatchJob(), m_Model, m_MsgSvc);
            }
            else
            {
                //TODO: open in new session or in current session
            }
        }

        private void CloseDocument() 
        {
            //TODO: check if is dirty and show warning
            Document = null;
        }
    }
}
