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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class BatchRunnerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_CachedTimeout;
        private bool m_IsBatchInProgress;
        private int m_ActiveTabIndex;
        private double m_Progress;

        private ICommand m_RunBatchCommand;
        private ICommand m_CancelBatchCommand;

        public ObservableCollection<string> Log { get; }
        
        public bool IsBatchInProgress
        {
            get => m_IsBatchInProgress;
            set
            {
                m_IsBatchInProgress = value;
                this.NotifyChanged();
            }
        }
        
        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

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

        public bool ContinueOnError 
        {
            get => m_Job.ContinueOnError;
            set 
            {
                m_Job.ContinueOnError = value;
                this.NotifyChanged();
            }
        }

        public int Timeout
        {
            get => m_Job.Timeout;
            set
            {
                m_CachedTimeout = value;
                m_Job.Timeout = value;
                this.NotifyChanged();
            }
        }

        public StartupOptions_e StartupOptions 
        {
            get => m_Job.StartupOptions;
            set 
            {
                m_Job.StartupOptions = value;
                this.NotifyChanged();
            }
        }

        public OpenFileOptions_e OpenFileOptions
        {
            get => m_Job.OpenFileOptions;
            set
            {
                m_Job.OpenFileOptions = value;
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

        public bool IsTimeoutEnabled
        {
            get => m_Job.Timeout != -1;
            set
            {                
                if (!value)
                {
                    m_Job.Timeout = -1;
                }
                else 
                {
                    m_Job.Timeout = m_CachedTimeout;
                }

                this.NotifyChanged();
            }
        }

        public ICommand RunBatchCommand => m_RunBatchCommand ?? (m_RunBatchCommand = new RelayCommand(RunBatch, () => !IsBatchInProgress && Input.Any() && Macros.Any()));
        public ICommand CancelBatchCommand => m_CancelBatchCommand ?? (m_CancelBatchCommand = new RelayCommand(CancelExport, () => IsBatchInProgress));
        
        public int ActiveTabIndex
        {
            get => m_ActiveTabIndex;
            set
            {
                m_ActiveTabIndex = value;
                this.NotifyChanged();
            }
        }

        private readonly IBatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        private readonly BatchJob m_Job;

        public BatchRunnerVM(IBatchRunnerModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;

            Log = new ObservableCollection<string>();

            m_Job = new BatchJob();
            m_CachedTimeout = m_Job.Timeout;

            m_Model.ProgressChanged += OnProgressChanged;
            m_Model.Log += OnLog;

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

        private void OnProgressChanged(double prg)
        {
            Progress = prg;
        }

        private void OnLog(string line)
        {
            Log.Add(line);
        }

        private async void RunBatch()
        {
            try
            {
                ActiveTabIndex = 2;
                IsBatchInProgress = true;
                Progress = 0;
                Log.Clear();

                if (await m_Model.BatchRun(m_Job).ConfigureAwait(false))
                {
                    m_MsgSvc.ShowInformation("Job completed successfully");
                }
                else 
                {
                    m_MsgSvc.ShowError("Job failed");
                }
            }
            catch(Exception ex)
            {
                //TODO: add log
                m_MsgSvc.ShowError(ex.ParseUserError(out _));
            }
            finally
            {
                IsBatchInProgress = false;
            }
        }
        
        private void CancelExport()
        {
            m_Model.Cancel();
        }
    }
}
