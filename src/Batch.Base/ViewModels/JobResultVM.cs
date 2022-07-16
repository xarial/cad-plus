//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XCad.Base;
using System.Threading;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;

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

        private readonly IXLogger m_Logger;

        private readonly CancellationTokenSource m_CancellationTokenSource;

        private bool m_IsRun;

        public JobResultVM(string name, IBatchRunJobExecutor executor, ICadDescriptor cadDesc,
            IXLogger logger, CancellationTokenSource cancellationTokenSource)
        {
            m_Executor = executor;
            CadDescriptor = cadDesc;

            m_Logger = logger;

            m_CancellationTokenSource = cancellationTokenSource;

            m_IsRun = false;

            Name = name;
            Summary = new JobResultSummaryVM(m_Executor, CadDescriptor);
            Journal = new JobResultJournalVM(m_Executor);
        }

        public void CancelJob() => m_CancellationTokenSource.Cancel();

        public async void TryRunBatchAsync()
        {
            if (!m_IsRun)
            {
                m_IsRun = true;

                try
                {
                    Status = JobState_e.InProgress;

                    IsBatchInProgress = true;

                    if (await m_Executor.ExecuteAsync(m_CancellationTokenSource.Token).ConfigureAwait(false))
                    {
                        UpdateJobStatus();
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
                catch (Exception ex)
                {
                    Status = JobState_e.Failed;
                    m_Logger.Log(ex);
                }
                finally
                {
                    IsBatchInProgress = false;
                }
            }
            else 
            {
                throw new NotSupportedException("Job result can only be executed once");
            }
        }

        public void TryRunBatch()
        {
            if (!m_IsRun)
            {
                m_IsRun = true;

                try
                {
                    Status = JobState_e.InProgress;

                    IsBatchInProgress = true;

                    if (m_Executor.Execute(m_CancellationTokenSource.Token))
                    {
                        UpdateJobStatus();
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
                catch (Exception ex)
                {
                    Status = JobState_e.Failed;
                    m_Logger.Log(ex);
                }
                finally
                {
                    IsBatchInProgress = false;
                }
            }
            else
            {
                throw new NotSupportedException("Job result can only be executed once");
            }
        }

        private void UpdateJobStatus()
        {
            if (Summary.JobItemFiles.All(f => f.Status == JobItemStatus_e.Succeeded))
            {
                Status = JobState_e.Succeeded;
            }
            else if (Summary.JobItemFiles.Any(f => f.Status == JobItemStatus_e.Succeeded))
            {
                Status = JobState_e.CompletedWithWarning;
            }
            else
            {
                Status = JobState_e.Failed;
            }
        }
    }
}
