using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
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
        
        public JobResultLogVM Log { get; }
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

        public JobResultVM(string name, IBatchRunJobExecutor executor)
        {
            m_Executor = executor;

            Name = name;
            Summary = new JobResultSummaryVM(m_Executor);
            Log = new JobResultLogVM(m_Executor);
        }

        private void CancelJob()
        {
            m_Executor.Cancel();
        }

        public async void RunBatchAsync()
        {
            try
            {
                Status = JobState_e.InProgress;

                IsBatchInProgress = true;

                if (await m_Executor.ExecuteAsync().ConfigureAwait(false))
                {
                    Status = Summary.JobItemFiles.Any(i => i.Status != Common.Services.JobItemStatus_e.Succeeded)
                        ? JobState_e.CompletedWithWarning : JobState_e.Succeeded;
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
