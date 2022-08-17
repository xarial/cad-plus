using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchJobItemState : IBatchJobItemState
    {
        public event BatchJobStateStatusChangedDelegate StatusChanged;
        public event BatchJobStateIssuesChangedDelegate IssuesChanged;

        public BatchJobItemStateStatus_e Status
        {
            get => m_Status;
            set
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        public IReadOnlyList<IBatchJobItemIssue> Issues => m_Issues;

        private BatchJobItemStateStatus_e m_Status;
        private readonly List<IBatchJobItemIssue> m_Issues;

        public BatchJobItemState()
        {
            m_Issues = new List<IBatchJobItemIssue>();
        }

        public void ReportError(Exception ex, string genericError = "Unknown error")
        {
            var err = "";

            if (ex is OperationCanceledException)
            {
                err = "Operation cancelled";
            }
            else if (ex is TimeoutException)
            {
                err = "Timeout";
            }
            else if (ex != null)
            {
                err = ex.ParseUserError(genericError);
            }

            ReportIssue(err, BatchJobItemIssueType_e.Error);
            Status = BatchJobItemStateStatus_e.Failed;
        }

        public void ReportIssue(string content, BatchJobItemIssueType_e type)
        {
            var issue = new BatchJobItemIssue(type, content);

            if (!m_Issues.Any(i => i.Equals(issue)))
            {
                m_Issues.Add(issue);
                IssuesChanged?.Invoke(this, Issues);
            }
        }

        public void ClearIssues()
        {
            m_Issues.Clear();
            IssuesChanged?.Invoke(this, Issues);
        }
    }
}
