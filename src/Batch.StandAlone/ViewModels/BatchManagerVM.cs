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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Batch.StandAlone.Properties;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.StandAlone.Controls;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.XBatch.Base;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
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

        public ICommand CreateDocumentCommand { get; }
        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand HelpCommand { get; }

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public IApplicationProvider[] AppProviders { get; }

        private readonly Func<System.IO.FileInfo, BatchJob, IApplicationProvider, MainWindow, BatchDocumentVM> m_OpenDocFunc;
        private readonly Func<string, BatchJob, IApplicationProvider, MainWindow, BatchDocumentVM> m_NewDocFunc;

        public BatchManagerVM(IApplicationProvider[] appProviders,
            IBatchRunnerModel model, IMessageService msgSvc, 
            Func<System.IO.FileInfo, BatchJob, IApplicationProvider, MainWindow, BatchDocumentVM> openDocFunc,
            Func<string, BatchJob, IApplicationProvider, MainWindow, BatchDocumentVM> newDocFunc)
        {
            AppProviders = appProviders;
            m_Model = model;
            m_MsgSvc = msgSvc;
            m_OpenDocFunc = openDocFunc;
            m_NewDocFunc = newDocFunc;

            CreateDocumentCommand = new RelayCommand<string>(CreateDocument);
            NewDocumentCommand = new RelayCommand(NewDocument);

            OpenDocumentCommand = new RelayCommand<string>(OpenDocument);
            CloseDocumentCommand = new RelayCommand(CloseDocument, () => Document != null);
            AboutCommand = new RelayCommand(ShowAbout);
            HelpCommand = new RelayCommand(OpenHelp);
        }

        private void NewDocument()
        {
            var newDocWnd = new NewDocumentWindow();
            newDocWnd.Owner = ParentWindow;
            newDocWnd.DataContext = this;
            newDocWnd.ShowDialog();
        }

        public ObservableCollection<string> RecentFiles => m_Model.RecentFiles;

        internal MainWindow ParentWindow { get; set; }

        internal bool CanClose()
        {
            if (Document?.IsDirty == true)
            {
                var res = m_MsgSvc.ShowQuestion("Document has unsaved changes. Do you want to save this document?");

                if (res.HasValue)
                {
                    if (res.Value)
                    {
                        Document.SaveDocument();
                    }

                    return true;
                }
                else 
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

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
                            Document = m_OpenDocFunc.Invoke(new System.IO.FileInfo(filePath), batchJob, GetApplicationProviderForJob(batchJob), ParentWindow);
                            Document.Save += OnSaveDocument;
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

        private void OnSaveDocument(BatchDocumentVM sender, BatchJob job, string filePath)
                    => m_Model.SaveJobToFile(job, filePath);

        internal void CreateDocument(string appId)
        {
            if (Document == null)
            {
                var job = m_Model.CreateNewJobDocument(appId);

                var provider = GetApplicationProviderForJob(job);
                Document = m_NewDocFunc.Invoke($"{provider.DisplayName} Batch+ Document", job, provider, ParentWindow);
                Document.Save += OnSaveDocument;
            }
            else
            {
                var args = Parser.Default.FormatCommandLine<FileOptions>(new FileOptions() 
                {
                    CreateNew = true,
                    ApplicationId = appId
                });
                StartNewInstance(args);
            }
        }

        private IApplicationProvider GetApplicationProviderForJob(BatchJob job) 
        {
            var appProvider = AppProviders.FirstOrDefault(
                p => string.Equals(p.ApplicationId, job.ApplicationId, 
                StringComparison.CurrentCultureIgnoreCase));

            if (appProvider == null) 
            {
                throw new UserException("Failed to find the application provider for this job file");
            }

            return appProvider;
        }

        private void CloseDocument() 
        {
            if (CanClose()) 
            {
                if (Document != null) 
                {
                    Document.Save -= OnSaveDocument;
                    Document = null;
                }
            }
        }

        private void ShowAbout() 
        {
            AboutDialog.Show(this.GetType().Assembly, Resources.batch_plus_icon,
                         new WindowInteropHelper(ParentWindow).Handle);
        }

        private void OpenHelp() 
        {
            try
            {
                Process.Start(Settings.Default.HelpLink);
            }
            catch 
            {
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
