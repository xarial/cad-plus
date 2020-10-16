using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultVM : IJobResult, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IJobResultLog IJobResult.Log => Log;
        IJobResultSummary IJobResult.Summary => Summary;

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
                IsBatchInProgress = true;
                
                if (await m_Executor.Execute().ConfigureAwait(false))
                {
                    //m_MsgSvc.ShowInformation("Job completed successfully");
                }
                else
                {
                    //m_MsgSvc.ShowError("Job failed");
                }
            }
            catch (Exception ex)
            {
                //TODO: add log
                //m_MsgSvc.ShowError(ex.ParseUserError(out _));
            }
            finally
            {
                IsBatchInProgress = false;
            }
        }
    }
}
