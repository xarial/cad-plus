//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.StandAlone.Controls;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Batch.Base;
using Xarial.CadPlus.Plus.UI;
using Xarial.CadPlus.Plus.Shared;
using Xarial.XCad.Base;
using Xarial.XToolkit;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Batch.StandAlone.Services;

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
        public ICommand OpenDocumentCommand { get; }

        public ICommand OpenInFileExplorerCommand { get; }

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        public ICadApplicationInstanceProvider[] AppProviders => m_BatchApp.ApplicationProviders;

        private readonly IBatchDocumentVMFactory m_BatchDocumentFactory;

        private readonly IAboutService m_AboutSvc;

        private readonly IBatchApplication m_BatchApp;

        public BatchManagerVM(IBatchApplication batchApp,
            IBatchRunnerModel model, IMessageService msgSvc, IXLogger logger,
            IBatchDocumentVMFactory batchDocVmFact, IAboutService aboutSvc)
        {
            m_BatchApp = batchApp;
            m_Model = model;
            m_MsgSvc = msgSvc;
            m_Logger = logger;
            m_BatchDocumentFactory = batchDocVmFact;

            m_AboutSvc = aboutSvc;

            CreateDocumentCommand = new RelayCommand<string>(CreateDocument);
            OpenDocumentCommand = new RelayCommand<string>(OpenDocument);

            OpenInFileExplorerCommand = new RelayCommand<string>(OpenInFileExplorer);
        }

        private void NewDocument()
        {
            //TODO: replace this with the service
            var newDocWnd = new NewDocumentWindow();
            newDocWnd.Owner = ParentWindow;
            newDocWnd.DataContext = this;
            newDocWnd.ShowDialog();
        }

        private void OpenInFileExplorer(string path)
        {
            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    FileSystemUtils.BrowseFolderInExplorer(path);
                }
                else if (System.IO.File.Exists(path))
                {
                    FileSystemUtils.BrowseFileInExplorer(path);
                }
            }
            catch
            {
            }
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
                        FileFilter.BuildFilterString(BatchDocumentVM.FileFilters)))
                {
                    if (!string.Equals(Document?.FilePath, filePath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (Document == null)
                        {
                            var batchJob = m_Model.LoadJobFromFile(filePath);
                            Document = m_BatchDocumentFactory.CreateOpen(new System.IO.FileInfo(filePath),
                                batchJob, ParentWindow,
                                GetBackstageCommands());
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
            catch (Exception ex)
            {
                m_Logger.Log(ex);
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

                Document = m_BatchDocumentFactory.CreateNew($"Batch+ Document",
                    job, ParentWindow, GetBackstageCommands());
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
            => m_AboutSvc.ShowAbout(this.GetType().Assembly, Resources.batch_plus_icon);

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
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError("Failed to start new instance of the application");
            }
        }

        private IRibbonButtonCommand[] GetBackstageCommands() 
        {
            return new IRibbonButtonCommand[]
            {
                new RibbonButtonCommand("New", Resources._new, "Create new Batch job", NewDocument, null),
                new RibbonButtonCommand("Open...", Resources.open, "Open existing batch job", () => OpenDocument(""), null),
                null,
                new RibbonButtonCommand("Save", Resources.save, "Save current batch job", () => Document.SaveDocument(), () => Document != null),
                new RibbonButtonCommand("Save As...", null, "Save current batch job to a new file", () => Document.SaveAsDocument(), () => Document != null),
                null,
                new RibbonButtonCommand("Close", null, "Close current batch job", CloseDocument, () => Document != null),
                null,
                new RibbonButtonCommand("Help...", Resources.help_icon, "Open help page", OpenHelp, null),
                new RibbonButtonCommand("About...", Resources.about_icon, "About Batch+", ShowAbout, null)
            };
        }
    }
}
