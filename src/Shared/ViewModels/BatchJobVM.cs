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
using Xarial.XCad.Base.Enums;
using Xarial.XToolkit;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
    public abstract class BatchJobBaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IBatchJobBase m_BatchJob;

        private readonly ObservableCollection<BatchJobItemVM> m_JobItems;
        private readonly ObservableCollection<BatchJobItemOperationDefinitionVM> m_OperationDefinitions;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        protected readonly CancellationTokenSource m_CancellationTokenSource;

        private object m_Lock;

        public ICommand CancelJobCommand { get; }

        public BatchJobStatus_e? Status => m_BatchJob.State?.Status;
        public double? Progress => m_BatchJob.State?.Progress;

        public ReadOnlyObservableCollection<BatchJobItemVM> JobItems { get; }
        public ReadOnlyObservableCollection<BatchJobItemOperationDefinitionVM> OperationDefinitions { get; }

        public int? TotalItemsCount => m_BatchJob?.State.TotalItemsCount;
        public int? SucceededItemsCount => m_BatchJob?.State.SucceededItemsCount;
        public int? WarningItemsCount => m_BatchJob?.State.WarningItemsCount;
        public int? FailedItemsCount => m_BatchJob?.State.FailedItemsCount;

        public DateTime? StartTime => m_BatchJob.State?.StartTime;
        public TimeSpan? Duration => m_BatchJob.State?.Duration;

        public ObservableCollection<string> LogEntries { get; }

        private IBatchJobReportExporter[] m_ReportExporters;
        private IBatchJobLogExporter[] m_LogExporters;

        public ICommand ExportReportCommand { get; }
        public ICommand ExportLogCommand { get; }

        protected BatchJobBaseVM(IBatchJobBase batchJob,
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

            LogEntries = new ObservableCollection<string>(batchJob.LogEntries ?? Enumerable.Empty<string>());
            BindingOperations.EnableCollectionSynchronization(LogEntries, m_Lock);

            m_JobItems = new ObservableCollection<BatchJobItemVM>((batchJob.JobItems ?? Enumerable.Empty<IBatchJobItem>()).Select(j => new BatchJobItemVM(j)));
            BindingOperations.EnableCollectionSynchronization(m_JobItems, m_Lock);

            m_OperationDefinitions = new ObservableCollection<BatchJobItemOperationDefinitionVM>(
                (batchJob.OperationDefinitions ?? Enumerable.Empty<IBatchJobItemOperationDefinition>()).Select(o => new BatchJobItemOperationDefinitionVM(o)));
            BindingOperations.EnableCollectionSynchronization(m_OperationDefinitions, m_Lock);

            JobItems = new ReadOnlyObservableCollection<BatchJobItemVM>(m_JobItems);
            BindingOperations.EnableCollectionSynchronization(JobItems, m_Lock);

            OperationDefinitions = new ReadOnlyObservableCollection<BatchJobItemOperationDefinitionVM>(m_OperationDefinitions);
            BindingOperations.EnableCollectionSynchronization(OperationDefinitions, m_Lock);

            CancelJobCommand = new RelayCommand(CancelJob, 
                () => !m_CancellationTokenSource.IsCancellationRequested
                    && (m_BatchJob.State.Status == BatchJobStatus_e.Initializing
                    || m_BatchJob.State.Status == BatchJobStatus_e.InProgress));

            m_BatchJob = batchJob;
            m_BatchJob.Started += OnJobStarted;
            m_BatchJob.Initialized += OnJobInitialized;
            m_BatchJob.ItemProcessed += OnItemProcessed;
            m_BatchJob.State.ProgressChanged += OnProgressChanged;
            m_BatchJob.Log += OnLog;
            m_BatchJob.Completed += OnJobCompleted;
            
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_CancellationTokenSource = cancellationTokenSource;
        }

        public void TryExportReport()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export report for job",
                        FileFilter.BuildFilterString(m_ReportExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
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

        public void TryExportLog()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export log for job",
                        FileFilter.BuildFilterString(m_LogExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
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

        private void OnProgressChanged(IBatchJobState sender, double progress)
        {
            this.NotifyChanged(nameof(Progress));
            this.NotifyChanged(nameof(Duration));
        }

        private void OnLog(IBatchJobBase sender, string line)
        {
            LogEntries.Add(line);
        }

        private void OnJobStarted(IBatchJobBase sender, DateTime startTime)
        {
            this.NotifyChanged(nameof(StartTime));
        }

        private void OnJobInitialized(IBatchJobBase sender, IReadOnlyList<IBatchJobItem> items, IReadOnlyList<IBatchJobItemOperationDefinition> operations)
        {
            m_Logger.Log($"Job initialized. Creating view models for {operations.Count} operation(s) and {items.Count} item(s)", LoggerMessageSeverity_e.Debug);

            foreach (var oper in operations) 
            {
                m_OperationDefinitions.Add(new BatchJobItemOperationDefinitionVM(oper));
            }

            foreach (var item in items) 
            {
                m_JobItems.Add(new BatchJobItemVM(item));
            }

            NotifyProcessedChanged();
            this.NotifyChanged(nameof(Status));
        }

        private void OnJobCompleted(IBatchJobBase sender, TimeSpan duration, BatchJobStatus_e status)
        {
            NotifyProcessedChanged();
            this.NotifyChanged(nameof(Status));
        }

        private void OnItemProcessed(IBatchJobBase sender, IBatchJobItem item)
        {
            NotifyProcessedChanged();
        }

        private void NotifyProcessedChanged()
        {
            this.NotifyChanged(nameof(TotalItemsCount));
            this.NotifyChanged(nameof(SucceededItemsCount));
            this.NotifyChanged(nameof(WarningItemsCount));
            this.NotifyChanged(nameof(FailedItemsCount));
            this.NotifyChanged(nameof(Duration));
        }

        public void CancelJob()
        {
            if (m_MsgSvc.ShowQuestion("Do you want to cancel this operation?") == true)
            {
                m_CancellationTokenSource.Cancel();
            }
        }
    }

    public class BatchJobVM : BatchJobBaseVM
    {
        private readonly IBatchJob m_BatchJob;

        public BatchJobVM(IBatchJob batchJob, IMessageService msgSvc, IXLogger logger,
            CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
            : base(batchJob, msgSvc, logger, cancellationTokenSource, reportExporters, logExporters)
        {
            m_BatchJob = batchJob;
        }

        public virtual void TryRunBatch()
        {
            m_BatchJob.TryExecute(m_CancellationTokenSource.Token);   
        }
    }

    public class AsyncBatchJobVM : BatchJobBaseVM
    {
        private readonly IAsyncBatchJob m_BatchJob;

        public AsyncBatchJobVM(IAsyncBatchJob batchJob, IMessageService msgSvc, IXLogger logger,
            CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
            : base(batchJob, msgSvc, logger, cancellationTokenSource, reportExporters, logExporters)
        {
            m_BatchJob = batchJob;
        }

        public virtual async Task TryRunBatchAsync()
        {
            await m_BatchJob.TryExecuteAsync(m_CancellationTokenSource.Token);
        }
    }
}
