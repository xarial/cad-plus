using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemState : IJobItemState
    {
        public event JobStateStatusChangedDelegate StatusChanged;
        public event JobStateIssuesChangedDelegate IssuesChanged;

        public JobItemStateStatus_e Status 
        {
            get => m_Status;
            set 
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        public IReadOnlyList<IJobItemIssue> Issues => m_Issues;

        private JobItemStateStatus_e m_Status;
        private readonly List<IJobItemIssue> m_Issues;

        public JobItemState() 
        {
            m_Issues = new List<IJobItemIssue>();
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

            ReportIssue(err, IssueType_e.Error);
            Status = JobItemStateStatus_e.Failed;
        }

        public void ReportIssue(string content, IssueType_e type)
        {
            var issue = new JobItemIssue(type, content);

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
