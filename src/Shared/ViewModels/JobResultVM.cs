using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Exceptions;
using Xarial.XCad.Base;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
    public abstract class JobResultBaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsBatchJobInProgress;

        private readonly IBatchJobBase m_BatchJob;

        private JobStatus_e m_Status;

        private int m_ProcessedItemsCount;
        private int m_FailedItemsCount;
        private DateTime? m_StartTime;
        private TimeSpan? m_Duration;

        private readonly List<JobItemVM> m_JobItems;
        private readonly List<JobItemOperationDefinitionVM> m_OperationDefinitions;

        private readonly IXLogger m_Logger;

        private readonly CancellationTokenSource m_CancellationTokenSource;

        private bool m_IsRun;

        private double m_Progress;
        private bool m_IsInitializing;

        private object m_Lock;

        public bool IsBatchJobInProgress
        {
            get => m_IsBatchJobInProgress;
            private set
            {
                m_IsBatchJobInProgress = value;
                this.NotifyChanged();
            }
        }

        public ICommand CancelJobCommand { get; }

        public JobStatus_e Status
        {
            get => m_Status;
            private set
            {
                m_Status = value;
                this.NotifyChanged();
            }
        }

        public double Progress
        {
            get => m_Progress;
            private set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public bool IsInitializing
        {
            get => m_IsInitializing;
            private set
            {
                if (value != m_IsInitializing)
                {
                    m_IsInitializing = value;
                    this.NotifyChanged();
                }
            }
        }

        public IReadOnlyList<JobItemVM> JobItems => IsInitializing ? null : m_JobItems;
        public IReadOnlyList<JobItemOperationDefinitionVM> OperationDefinitions => IsInitializing ? null : m_OperationDefinitions;

        public int ProcessedItemsCount
        {
            get => m_ProcessedItemsCount;
            private set
            {
                m_ProcessedItemsCount = value;
                this.NotifyChanged();
            }
        }

        public int FailedItemsCount
        {
            get => m_FailedItemsCount;
            private set
            {
                m_FailedItemsCount = value;
                this.NotifyChanged();
            }
        }

        public DateTime? StartTime
        {
            get => m_StartTime;
            private set
            {
                m_StartTime = value;
                this.NotifyChanged();
            }
        }

        public TimeSpan? Duration
        {
            get => m_Duration;
            private set
            {
                m_Duration = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<string> Output { get; }

        public JobResultBaseVM(IBatchJobBase batchJob,
            IXLogger logger, CancellationTokenSource cancellationTokenSource)
        {
            if (batchJob == null) 
            {
                throw new ArgumentNullException(nameof(batchJob));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            m_Lock = new object();

            Output = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(Output, m_Lock);

            m_JobItems = new List<JobItemVM>();
            BindingOperations.EnableCollectionSynchronization(m_JobItems, m_Lock);

            m_OperationDefinitions = new List<JobItemOperationDefinitionVM>();
            BindingOperations.EnableCollectionSynchronization(m_OperationDefinitions, m_Lock);

            CancelJobCommand = new RelayCommand(CancelJob, () => IsBatchJobInProgress);

            m_BatchJob = batchJob;
            m_BatchJob.JobSet += OnJobSet;
            m_BatchJob.ItemProcessed += OnProgressChanged;
            m_BatchJob.JobCompleted += OnJobCompleted;
            m_BatchJob.Log += OnLog;

            m_Logger = logger;

            m_CancellationTokenSource = cancellationTokenSource;

            m_IsRun = false;

            IsInitializing = true;
        }

        private void OnLog(IBatchJobBase sender, string line)
        {
            Output.Add(line);
        }

        private void OnJobSet(IBatchJobBase sender, IReadOnlyList<IJobItem> items, IReadOnlyList<IJobItemOperationDefinition> operations, DateTime startTime)
        {
            m_JobItems.AddRange(items.Select(f => new JobItemVM(f)));
            m_OperationDefinitions.AddRange(operations.Select(o => new JobItemOperationDefinitionVM(o)));
            StartTime = startTime;
            IsInitializing = false;

            this.NotifyChanged(nameof(JobItems));
            this.NotifyChanged(nameof(OperationDefinitions));
        }

        private void OnJobCompleted(IBatchJobBase sender, TimeSpan duration)
        {
            Duration = duration;
        }

        private void OnProgressChanged(IBatchJobBase sender, IJobItem file, double progress, bool result)
        {
            if (result)
            {
                ProcessedItemsCount++;
            }
            else
            {
                FailedItemsCount++;
            }

            Progress = progress;

            if (StartTime.HasValue)
            {
                Duration = DateTime.Now - StartTime.Value;
            }
            else
            {
                Debug.Assert(false, "Start time must be set before progress");
            }
        }

        public void CancelJob() => m_CancellationTokenSource.Cancel();

        private void UpdateJobStatus()
        {
            var allOperations = (m_BatchJob.JobItems ?? new IJobItem[0]).SelectMany(i => i.Operations ?? new IJobItemOperation[0]);

            if (allOperations.All(f => f.State.Status == JobItemStateStatus_e.Succeeded))
            {
                Status = JobStatus_e.Succeeded;
            }
            else if (allOperations.Any(f => f.State.Status == JobItemStateStatus_e.Succeeded))
            {
                Status = JobStatus_e.CompletedWithWarning;
            }
            else
            {
                Status = JobStatus_e.Failed;
            }
        }

        protected CancellationToken StartRun() 
        {
            if (!m_IsRun)
            {
                m_IsRun = true;

                Status = JobStatus_e.InProgress;

                IsBatchJobInProgress = true;

                return m_CancellationTokenSource.Token;
            }
            else
            {
                throw new NotSupportedException("Job result can only be executed once");
            }
        }

        protected void FinishRun(bool res, Exception err) 
        {
            if (err == null)
            {
                if (res)
                {
                    UpdateJobStatus();
                }
                else
                {
                    Status = JobStatus_e.Failed;
                }
            }
            else 
            {
                switch (err) 
                {
                    case JobCancelledException _:
                        Status = JobStatus_e.Cancelled;
                        break;

                    case Exception ex:
                        Status = JobStatus_e.Failed;
                        m_Logger.Log(ex);
                        break;
                }
            }

            IsBatchJobInProgress = false;
        }
    }

    public class JobResultVM : JobResultBaseVM
    {
        private readonly IBatchJob m_BatchJob;

        public JobResultVM(IBatchJob batchJob, IXLogger logger,
            CancellationTokenSource cancellationTokenSource) : base(batchJob, logger, cancellationTokenSource)
        {
            m_BatchJob = batchJob;
        }

        public void TryRunBatch()
        {
            var cancellationToken = StartRun();

            try
            {
                var res = m_BatchJob.Execute(cancellationToken);
                FinishRun(res, null);
            }
            catch (Exception ex)
            {
                FinishRun(false, ex);
            }
        }
    }

    public class AsyncJobResultVM : JobResultBaseVM
    {
        private readonly IAsyncBatchJob m_BatchJob;

        public AsyncJobResultVM(IAsyncBatchJob batchJob, IXLogger logger,
            CancellationTokenSource cancellationTokenSource) : base(batchJob, logger, cancellationTokenSource)
        {
            m_BatchJob = batchJob;
        }

        public async Task TryRunBatchAsync()
        {
            var cancellationToken = StartRun();
            
            try
            {
                var res = await m_BatchJob.ExecuteAsync(cancellationToken);
                FinishRun(res, null);
            }
            catch (Exception ex)
            {
                FinishRun(false, ex);
            }
        }
    }
}
