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
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Exceptions;
using Xarial.XCad.Base;
using Xarial.XToolkit;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

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

        private readonly IMessageService m_MsgSvc;
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

        public ObservableCollection<string> LogEntries { get; }

        private IBatchJobReportExporter[] m_ReportExporters;
        private IBatchJobLogExporter[] m_LogExporters;

        public ICommand ExportReportCommand { get; }
        public ICommand ExportLogCommand { get; }

        public JobResultBaseVM(IBatchJobBase batchJob,
            IMessageService msgSvc, IXLogger logger, CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
        {
            if (batchJob == null) 
            {
                throw new ArgumentNullException(nameof(batchJob));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (msgSvc == null)
            {
                throw new ArgumentNullException(nameof(msgSvc));
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            m_ReportExporters = reportExporters;
            m_LogExporters = logExporters;

            ExportReportCommand = new RelayCommand(TryExportReport, () => m_ReportExporters?.Any() == true);
            ExportLogCommand = new RelayCommand(TryExportLog, () => m_LogExporters?.Any() == true);

            m_Lock = new object();

            LogEntries = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(LogEntries, m_Lock);

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

            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_CancellationTokenSource = cancellationTokenSource;

            m_IsRun = false;

            IsInitializing = true;
        }

        private void TryExportReport()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export report for job",
                        FileFilter.Combine(m_ReportExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
                {
                    var reportWriter = m_ReportExporters.FirstOrDefault(j => TextUtils.MatchesAnyFilter(filePath, j.Filter.Extensions));

                    if (reportWriter != null)
                    {
                        reportWriter.Export(m_BatchJob, filePath);
                    }
                    else
                    {
                        throw new UserException("Unrecognized file format of job report");
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private void TryExportLog()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export log for job",
                        FileFilter.Combine(m_ReportExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
                {
                    var logWriter = m_LogExporters.FirstOrDefault(j => TextUtils.MatchesAnyFilter(filePath, j.Filter.Extensions));

                    if (logWriter != null)
                    {
                        logWriter.Export(m_BatchJob, filePath);
                    }
                    else
                    {
                        throw new UserException("Unrecognized file format of log");
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private void OnLog(IBatchJobBase sender, string line)
        {
            LogEntries.Add(line);
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

        public JobResultVM(IBatchJob batchJob, IMessageService msgSvc, IXLogger logger,
            CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
            : base(batchJob, msgSvc, logger, cancellationTokenSource, reportExporters, logExporters)
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

        public AsyncJobResultVM(IAsyncBatchJob batchJob, IMessageService msgSvc, IXLogger logger,
            CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
            : base(batchJob, msgSvc, logger, cancellationTokenSource, reportExporters, logExporters)
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
