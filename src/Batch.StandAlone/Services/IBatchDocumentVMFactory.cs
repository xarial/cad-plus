using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.Base;
using Xarial.XToolkit.Services;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IBatchDocumentVMFactory
    {
        BatchDocumentVM CreateOpen(FileInfo fileInfo, BatchJob job, MainWindow parentWindow, IRibbonButtonCommand[] ribbonButtonCommands);
        BatchDocumentVM CreateNew(string name, BatchJob job, MainWindow parentWindow, IRibbonButtonCommand[] ribbonButtonCommands);
    }

    public class BatchDocumentVMFactory : IBatchDocumentVMFactory
    {
        private readonly IMessageService m_MsgSvc;

        private readonly IBatchRunJobExecutorFactory m_ExecFact;
        private readonly ICadApplicationInstanceProvider[] m_AppProviders;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        private readonly IXLogger m_Logger;

        private readonly IJournalExporter[] m_JournalExporters;
        private readonly IResultsSummaryExcelExporter[] m_ResultsExporters;

        public BatchDocumentVMFactory(ICadApplicationInstanceProvider[] appProviders,
            IEnumerable<IJournalExporter> journalExporters, IEnumerable<IResultsSummaryExcelExporter> resultsExporters,
            IMessageService msgSvc, IXLogger logger, IBatchRunJobExecutorFactory execFact,
            IBatchApplicationProxy batchAppProxy)
        {
            m_ExecFact = execFact;
            m_AppProviders = appProviders;

            m_JournalExporters = journalExporters.ToArray();
            m_ResultsExporters = resultsExporters.ToArray();

            m_MsgSvc = msgSvc;
            m_Logger = logger;
            m_BatchAppProxy = batchAppProxy;
        }

        public BatchDocumentVM CreateNew(string name, BatchJob job, MainWindow parentWindow, IRibbonButtonCommand[] ribbonButtonCommands)
            => new BatchDocumentVM(name, job, job.FindApplicationProvider(m_AppProviders), m_JournalExporters,
                m_ResultsExporters, m_MsgSvc, m_Logger, m_ExecFact, m_BatchAppProxy, parentWindow, ribbonButtonCommands);

        public BatchDocumentVM CreateOpen(FileInfo fileInfo, BatchJob job, MainWindow parentWindow, IRibbonButtonCommand[] ribbonButtonCommands)
            => new BatchDocumentVM(fileInfo, job, job.FindApplicationProvider(m_AppProviders), m_JournalExporters,
                m_ResultsExporters, m_MsgSvc, m_Logger, m_ExecFact, m_BatchAppProxy, parentWindow, ribbonButtonCommands);
    }
}
