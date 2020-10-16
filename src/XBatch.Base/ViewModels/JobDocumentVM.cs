using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobDocumentVM : INotifyPropertyChanged, IJobDocument
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

        IJobSettings IJobDocument.Settings => Settings;
        IJobResults IJobDocument.Results => Results;
        
        public JobSettingsVM Settings { get; }
        public JobResultsVM Results { get; }

        private ICommand m_RunBatchCommand;
        
        public ObservableCollection<string> Input { get; }

        public ObservableCollection<string> Macros { get; }

        public string Filter
        {
            get => m_Job.Filter;
            set
            {
                m_Job.Filter = value;
                this.NotifyChanged();
            }
        }

        public AppVersionInfo Version
        {
            get => m_Job.Version;
            set
            {
                m_Job.Version = value;
                this.NotifyChanged();
            }
        }

        public AppVersionInfo[] InstalledVersions { get; set; }

        public FileFilter[] InputFilesFilter => m_Model.InputFilesFilter;

        public FileFilter[] MacroFilesFilter => m_Model.MacroFilesFilter;

        public ICommand RunBatchCommand => m_RunBatchCommand ?? (m_RunBatchCommand = new RelayCommand(RunBatch, () => Input.Any() && Macros.Any()));

        private readonly IBatchRunnerModel m_Model;
        private readonly BatchJob m_Job;

        public JobDocumentVM(string name, BatchJob job, IBatchRunnerModel model) 
        {
            m_Model = model;
            m_Job = job;

            Name = name;
            Settings = new JobSettingsVM(m_Job);
            Results = new JobResultsVM(m_Model, m_Job);
            Input = new ObservableCollection<string>();
            Input.CollectionChanged += OnInputCollectionChanged;

            Macros = new ObservableCollection<string>();
            Macros.CollectionChanged += OnMacrosCollectionChanged;

            InstalledVersions = m_Model.InstalledVersions;
            Version = InstalledVersions.FirstOrDefault();
        }

        private void OnInputCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Input = Input.ToArray();
        }

        private void OnMacrosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Macros = Macros.ToArray();
        }

        private void RunBatch() 
        {
            Results.StartNewJob();
        }
    }
}
