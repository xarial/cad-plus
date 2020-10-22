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
    public class BatchDocumentVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_Name;

        public string Name
        {
            get => m_Name;
            private set 
            {
                m_Name = value;
                this.NotifyChanged();
            }
        }
                
        public BatchDocumentSettingsVM Settings { get; }
        public JobResultsVM Results { get; }

        public ObservableCollection<string> Input { get; }

        public ObservableCollection<string> Macros { get; }

        public string Filter
        {
            get => m_Job.Filter;
            set
            {
                m_Job.Filter = value;
                this.NotifyChanged();
                IsDirty = true;
            }
        }

        public FileFilter[] InputFilesFilter => m_Model.InputFilesFilter;

        public FileFilter[] MacroFilesFilter => m_Model.MacroFilesFilter;

        public ICommand RunJobCommand { get; }

        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveAsDocumentCommand { get; }

        public bool IsDirty 
        {
            get => m_IsDirty;
            private set
            {
                m_IsDirty = value;
                this.NotifyChanged();
            }
        }

        private readonly IBatchRunnerModel m_Model;
        private readonly BatchJob m_Job;
        private readonly IMessageService m_MsgSvc;

        private bool m_IsDirty;
        private string m_FilePath;

        public string FilePath => m_FilePath;

        public BatchDocumentVM(FileInfo file, BatchJob job, IBatchRunnerModel model, IMessageService msgSvc) 
            : this(Path.GetFileNameWithoutExtension(file.FullName), job, model, msgSvc)
        {
            m_FilePath = file.FullName;
            IsDirty = false;
        }

        public BatchDocumentVM(string name, BatchJob job, IBatchRunnerModel model, IMessageService msgSvc) 
        {
            m_Model = model;
            m_Job = job;
            m_MsgSvc = msgSvc;

            IsDirty = true;

            RunJobCommand = new RelayCommand(RunJob, () => Input.Any() && Macros.Any() && Settings.Version != null);
            SaveDocumentCommand = new RelayCommand(SaveDocument, () => IsDirty);
            SaveAsDocumentCommand = new RelayCommand(SaveAsDocument);

            Name = name;
            Settings = new BatchDocumentSettingsVM(m_Job, model);
            Settings.Modified += OnSettingsModified;
            Results = new JobResultsVM(m_Model, m_Job);
            Input = new ObservableCollection<string>(m_Job.Input ?? Enumerable.Empty<string>());
            Input.CollectionChanged += OnInputCollectionChanged;

            Macros = new ObservableCollection<string>(m_Job.Macros ?? Enumerable.Empty<string>());
            Macros.CollectionChanged += OnMacrosCollectionChanged;
        }

        private void OnSettingsModified()
        {
            IsDirty = true;
        }

        private void SaveAsDocument()
        {
            if (FileSystemBrowser.BrowseFileSave(out m_FilePath, 
                "Select file path", 
                FileSystemBrowser.BuildFilterString(new FileFilter("xBatch File", "*.xbatch"), FileFilter.AllFiles))) 
            {
                SaveDocument();
                Name = Path.GetFileNameWithoutExtension(m_FilePath);
            }
        }

        public void SaveDocument()
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                SaveAsDocument();
                return;
            }

            try
            {
                m_Model.SaveJobToFile(m_Job, m_FilePath);
                IsDirty = false;
            }
            catch
            {
                m_MsgSvc.ShowError("Failed to save document");
            }
        }

        private void OnInputCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Input = Input.ToArray();
            IsDirty = true;
        }

        private void OnMacrosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Macros = Macros.ToArray();
            IsDirty = true;
        }

        private void RunJob() 
        {
            Results.StartNewJob();
        }
    }
}
