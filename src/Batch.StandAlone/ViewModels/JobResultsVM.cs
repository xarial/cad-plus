//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XCad.Base;
using Xarial.CadPlus.Batch.StandAlone.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using System.Threading;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class AsyncBatchStandAloneJobResultVM : AsyncBatchJobVM
    {
        public string Name { get; }

        public AsyncBatchStandAloneJobResultVM(string name, IAsyncBatchJob batchJob, IMessageService msgSvc, IXLogger logger, CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters) 
            : base(batchJob, msgSvc, logger, cancellationTokenSource, reportExporters, logExporters)
        {
            Name = name;
        }
    }

    public class JobResultsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AsyncBatchStandAloneJobResultVM m_Selected;

        public AsyncBatchStandAloneJobResultVM Selected 
        {
            get => m_Selected;
            set 
            {
                m_Selected = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<BatchJobBaseVM> Items { get; }

        private readonly BatchJob m_Job;

        private readonly IBatchMacroRunJobStandAloneFactory m_JobFact;

        private readonly IXLogger m_Logger;

        private readonly IMessageService m_MsgSvc;

        private readonly IBatchJobReportExporter[] m_ReportExporters;
        private readonly IBatchJobLogExporter[] m_LogExporters;

        public JobResultsVM(BatchJob job,
            IBatchMacroRunJobStandAloneFactory jobFact, IXLogger logger, IMessageService msgSvc, IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters) 
        {
            m_Job = job;

            m_Logger = logger;
            m_MsgSvc = msgSvc;

            m_ReportExporters = reportExporters;
            m_LogExporters = logExporters;

            m_JobFact = jobFact;
            Items = new ObservableCollection<BatchJobBaseVM>();
        }

        public async void StartNewJob()
        {
            var newRes = new AsyncBatchStandAloneJobResultVM($"Job #{Items.Count + 1}", m_JobFact.Create(m_Job), m_MsgSvc, m_Logger, new CancellationTokenSource(),
                m_ReportExporters, m_LogExporters);
            Items.Add(newRes);
            Selected = newRes;
            await newRes.TryRunBatchAsync();
        }
    }
}
