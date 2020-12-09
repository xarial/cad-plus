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
using System.Windows.Controls;
using System.Windows.Input;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class FilterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string[] m_Src;
        private int m_Index;
        private string m_Value;

        public string Value
        {
            get => m_Value;
            set 
            {
                m_Value = value;
                m_Src[m_Index] = value;
                this.NotifyChanged();
            }
        }

        public FilterVM() : this("*.*")
        {
        }

        public FilterVM(string value) 
        {
            m_Value = value;
        }

        internal void SetBinding(string[] src, int index) 
        {
            m_Src = src;
            m_Index = index;
        }
    }

    public class BatchDocumentVM : INotifyPropertyChanged
    {
        internal static FileFilter[] FileFilters { get; }
            = new FileFilter[] 
            {
                new FileFilter("Batch+ Job File", "*.bpj"),
                FileFilter.AllFiles };

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

        public ObservableCollection<MacroData> Macros { get; }

        public ObservableCollection<FilterVM> Filters { get; }

        public FileFilter[] InputFilesFilter => m_Model.InputFilesFilter;

        public FileFilter[] MacroFilesFilter => m_Model.MacroFilesFilter;

        public ICommand RunJobCommand { get; }

        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveAsDocumentCommand { get; }

        public ICommand FilterEditEndingCommand { get; }

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

            RunJobCommand = new RelayCommand(RunJob, () => CanRunJob);
            SaveDocumentCommand = new RelayCommand(SaveDocument, () => IsDirty);
            SaveAsDocumentCommand = new RelayCommand(SaveAsDocument);
            FilterEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(FilterEditEnding);

            Name = name;
            Settings = new BatchDocumentSettingsVM(m_Job, model);
            Settings.Modified += OnSettingsModified;
            Results = new JobResultsVM(m_Model, m_Job);

            Filters = new ObservableCollection<FilterVM>((m_Job.Filters ?? Enumerable.Empty<string>()).Select(f => new FilterVM(f)));
            Filters.CollectionChanged += OnFiltersCollectionChanged;
            BindFilters();

            Input = new ObservableCollection<string>(m_Job.Input ?? Enumerable.Empty<string>());
            Input.CollectionChanged += OnInputCollectionChanged;

            Macros = new ObservableCollection<MacroData>(m_Job.Macros ?? Enumerable.Empty<MacroData>());
            Macros.CollectionChanged += OnMacrosCollectionChanged;
        }

        public Func<string, object> PathToMacroDataConverter { get; }
            = new Func<string, object>(p => new MacroData() { FilePath = p });

        public Func<object, string> MacroDataToPathConverter { get; }
        = new Func<object, string>((m) => ((MacroData)m).FilePath);

        private bool CanRunJob => Input.Any() && Macros.Any() && Settings.Version != null;

        private void FilterEditEnding(DataGridCellEditEndingEventArgs args)
        {
            var curFilter = args.EditingElement.DataContext as FilterVM;

            for (int i = Filters.Count - 1; i >= 0; i--)
            {
                if (Filters[i] != curFilter &&
                    string.Equals(Filters[i].Value, curFilter.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    Filters.RemoveAt(i);
                }
            }
        }

        private void OnSettingsModified()
        {
            IsDirty = true;
        }

        private void SaveAsDocument()
        {
            if (FileSystemBrowser.BrowseFileSave(out m_FilePath, 
                "Select file path",
                FileSystemBrowser.BuildFilterString(FileFilters)))
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

        private void OnFiltersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindFilters();
            IsDirty = true;
        }

        private void BindFilters() 
        {
            m_Job.Filters = Filters.Select(f => f.Value).ToArray();

            for (int i = 0; i < Filters.Count; i++) 
            {
                Filters[i].SetBinding(m_Job.Filters, i);
            }
        }

        internal void RunJob() 
        {
            if (!CanRunJob) 
            {
                throw new UserMessageException("Cannot run this job as preconditions are not met");
            }

            Results.StartNewJob();
        }
    }
}
