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

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class BatchRunnerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsBatchInProgress;
        private int m_ActiveTabIndex;
        private double m_Progress;
        private bool m_IsTimeoutEnabled;

        private ICommand m_RunBatchCommand;
        private ICommand m_CancelBatchCommand;
        private ICommand m_AddMacroCommand;

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

        public string Filter { get; set; }

        public bool ContinueOnError { get; set; }

        public int Timeout { get; set; }

        public bool IsTimeoutEnabled
        {
            get => m_IsTimeoutEnabled;
            set
            {
                m_IsTimeoutEnabled = value;
                this.NotifyChanged();
            }
        }

        public ICommand ExportCommand => m_RunBatchCommand ?? (m_RunBatchCommand = new RelayCommand(RunBatch, () => !IsBatchInProgress && Input.Any()));
        public ICommand CancelBatchCommand => m_CancelBatchCommand ?? (m_CancelBatchCommand = new RelayCommand(CancelExport, () => IsBatchInProgress));
        public ICommand AddMacroCommand => m_AddMacroCommand ?? (m_AddMacroCommand = new RelayCommand(AddMacro, () => !IsBatchInProgress));

        public int ActiveTabIndex
        {
            get => m_ActiveTabIndex;
            set
            {
                m_ActiveTabIndex = value;
                this.NotifyChanged();
            }
        }

        private readonly BatchRunnerModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public BatchRunnerVM(BatchRunnerModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;

            Log = new ObservableCollection<string>();

            m_Model.ProgressChanged += OnProgressChanged;
            m_Model.Log += OnLog;
            Input = new ObservableCollection<string>();
            Macros = new ObservableCollection<string>();
            Filter = "*.*";
            IsTimeoutEnabled = true;
            Timeout = 600;
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
                ActiveTabIndex = 1;
                IsBatchInProgress = true;
                Progress = 0;
                Log.Clear();

                var opts = new BatchRunnerOptions()
                {
                    Input = Input?.ToArray(),
                    Macros = Macros?.ToArray(),
                    Filter = Filter,
                    ContinueOnError = ContinueOnError,
                    Timeout = IsTimeoutEnabled ? Timeout : -1
                };

                await m_Model.BatchRun(opts).ConfigureAwait(false);

                m_MsgSvc.ShowInformation("Operation completed");
            }
            catch
            {
                m_MsgSvc.ShowError("Processing error");
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

        private void AddMacro()
        {
            if (FileSystemBrowser.BrowseFileOpen(out string macroPath, 
                "Select macro file", 
                FileSystemBrowser.BuildFilterString(new FileFilter("SOLIDWORKS Macro", "*.swp"), FileFilter.AllFiles))) 
            {
                Macros.Add(macroPath);
            }
        }
    }
}
