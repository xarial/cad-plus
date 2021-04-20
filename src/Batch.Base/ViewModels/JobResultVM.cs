//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public enum JobState_e 
    {
        InProgress,
        Failed,
        Succeeded,
        CompletedWithWarning,
        Cancelled
    }

    public class JobResultVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public JobResultJournalVM Journal { get; }
        public JobResultSummaryVM Summary { get; }

        public string Name { get; }

        private ICommand m_CancelJobCommand;

        public bool IsBatchInProgress
        {
            get => m_IsBatchInProgress;
            set
            {
                m_IsBatchInProgress = value;
                this.NotifyChanged();
            }
        }

        public ICommand CancelJobCommand => m_CancelJobCommand ?? (m_CancelJobCommand = new RelayCommand(CancelJob, () => IsBatchInProgress));

        private bool m_IsBatchInProgress;
        
        private readonly IBatchRunJobExecutor m_Executor;

        private JobState_e m_Status;

        public JobState_e Status
        {
            get => m_Status;
            set 
            {
                m_Status = value;
                this.NotifyChanged();
            }
        }

        public ICadDescriptor CadDescriptor { get; }

        public JobResultVM(string name, IBatchRunJobExecutor executor, ICadDescriptor cadDesc)
        {
            m_Executor = executor;
            CadDescriptor = cadDesc;
            
            Name = name;
            Summary = new JobResultSummaryVM(m_Executor);
            Journal = new JobResultJournalVM(m_Executor);
        }

        public void CancelJob()
            => m_Executor.Cancel();

        public async void RunBatchAsync()
        {
            try
            {
                Status = JobState_e.InProgress;

                IsBatchInProgress = true;

                if (await m_Executor.ExecuteAsync().ConfigureAwait(false))
                {
                    if (Summary.JobItemFiles.All(f => f.Status == Common.Services.JobItemStatus_e.Succeeded))
                    {
                        Status = JobState_e.Succeeded;
                    }
                    else if (Summary.JobItemFiles.Any(f => f.Status == Common.Services.JobItemStatus_e.Succeeded))
                    {
                        Status = JobState_e.CompletedWithWarning;
                    }
                    else 
                    {
                        Status = JobState_e.Failed;
                    }
                }
                else
                {
                    Status = JobState_e.Failed;
                }
            }
            catch (JobCancelledException) 
            {
                Status = JobState_e.Cancelled;
            }
            catch (Exception)
            {
                Status = JobState_e.Failed;
            }
            finally
            {
                IsBatchInProgress = false;
            }
        }
    }
}
