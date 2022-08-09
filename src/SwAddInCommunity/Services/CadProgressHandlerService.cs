﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.AddIn.Sw.UI;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.AddIn.Sw.Services
{
    public class CadBatchJobHandlerServiceFactory : IBatchJobHandlerServiceFactory
    {
        private readonly IXExtension m_Ext;
        private readonly IXLogger m_Logger;

        private readonly IMessageService m_MsgSvc;

        private readonly IBatchJobReportExporter[] m_ReportExporters;
        private readonly IBatchJobLogExporter[] m_LogExporters;

        public CadBatchJobHandlerServiceFactory(IXExtension ext, IMessageService msgSvc, IXLogger logger,
            IEnumerable<IBatchJobReportExporter> reportExporters, IEnumerable<IBatchJobLogExporter> logExporters)
        {
            m_Ext = ext;
            m_Logger = logger;
            m_MsgSvc = msgSvc;

            m_ReportExporters = reportExporters.ToArray();
            m_LogExporters = logExporters.ToArray();
        }

        public IBatchJobHandlerService Create(IBatchJob job, string title, CancellationTokenSource cancellationTokenSource)
            => new CadBatchJobHandlerService(job, m_Logger, m_MsgSvc, m_Ext, title, cancellationTokenSource, m_ReportExporters, m_LogExporters);
    }

    public class CadBatchJobHandlerService : IBatchJobHandlerService
    {
        private readonly IXExtension m_Ext;
        private readonly IXLogger m_Logger;
        private readonly IXProgress m_Progress;
        private readonly IBatchJob m_Job;

        private readonly CancellationTokenSource m_CancellationTokenSource;
        private readonly JobResultVM m_JobResult;

        private readonly IXPopupWindow<ResultsWindow> m_ResultsWindow;

        public CadBatchJobHandlerService(IBatchJob job, IXLogger logger, IMessageService msgSvc, IXExtension ext, 
            string title, CancellationTokenSource cancellationTokenSource,
            IBatchJobReportExporter[] reportExporters, IBatchJobLogExporter[] logExporters)
        {
            m_Job = job;
            m_Job.ItemProcessed += OnJobItemProcessed;
            m_Logger = logger;
            m_Ext = ext;

            m_Progress = m_Ext.Application.CreateProgress();
            m_CancellationTokenSource = cancellationTokenSource;

            m_JobResult = new JobResultVM(m_Job, msgSvc, m_Logger, m_CancellationTokenSource, reportExporters, logExporters);

            m_ResultsWindow = m_Ext.CreatePopupWindow<ResultsWindow>();
            m_ResultsWindow.Control.Title = title;
            m_ResultsWindow.Control.DataContext = m_JobResult;
        }

        public void Run() 
        {
            m_JobResult.TryRunBatch();
            m_ResultsWindow.ShowDialog();
        }

        private void OnJobItemProcessed(IBatchJobBase sender, IJobItem item, double progress, bool result)
        {
            m_Progress.Report(progress);
        }

        public void Dispose()
        {
            m_Progress.Dispose();
            m_CancellationTokenSource.Dispose();
        }
    }
}
