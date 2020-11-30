//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
            OpenDocumentCommand = new RelayCommand<string>(OpenDocument);
            CloseDocumentCommand = new RelayCommand(CloseDocument, () => Document != null);
        }

        public ObservableCollection<string> RecentFiles => m_Model.RecentFiles;
        
        internal void OpenDocument(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) ||
                    FileSystemBrowser.BrowseFileOpen(out filePath, "Select file to open",
                        FileSystemBrowser.BuildFilterString(BatchDocumentVM.FileFilters)))
                {
                    if (!string.Equals(Document?.FilePath, filePath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Document == null)
                        {
                            var batchJob = m_Model.LoadJobFromFile(filePath);
                            Document = new BatchDocumentVM(new FileInfo(filePath), batchJob, m_Model, m_MsgSvc);
                        }
                        else
                        {
                            var args = Parser.Default.FormatCommandLine<FileOptions>(new FileOptions() 
                            {
                                FilePath = filePath 
                            });

                            StartNewInstance(args);
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

        internal void NewDocument() 
        {
            if (Document == null)
            {
                var job = m_Model.CreateNewJobDocument();

                Document = new BatchDocumentVM("Batch+ Document", job, m_Model, m_MsgSvc);
            }
            else
            {
                var args = Parser.Default.FormatCommandLine<FileOptions>(new FileOptions() { CreateNew = true });
                StartNewInstance(args);
            }
        }

        private void CloseDocument() 
        {
            if (Document.IsDirty)
            {
                var res = m_MsgSvc.ShowQuestion("Document has unsaved changes. Do you want to save this document?");

                if (res.HasValue)
                {
                    if (res.Value) 
                    {
                        Document.SaveDocument();
                    }

                    Document = null;
                }
            }
            else 
            {
                Document = null;
            }
        }

        private void StartNewInstance(string args)
        {
            try
            {
                var appPath = Process.GetCurrentProcess().MainModule.FileName;
                var prcStartInfo = new ProcessStartInfo(appPath, args);
                Process.Start(prcStartInfo);
            }
            catch 
            {
                m_MsgSvc.ShowError("Failed to start new instance of the application");
            }
        }
    }
}
