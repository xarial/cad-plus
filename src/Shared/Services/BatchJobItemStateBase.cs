//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public abstract class BatchJobItemStateBase : IBatchJobItemStateBase
    {
        public BatchJobItemStateStatus_e Status
        {
            get => m_Status;
            set
            {
                m_Status = value;
                RaiseStatusChanged(value);
            }
        }

        public IReadOnlyList<IBatchJobItemIssue> Issues 
        {
            get => m_Issues;
            set 
            {
                m_Issues.Clear();

                if (value != null) 
                {
                    foreach (var issue in value) 
                    {
                        m_Issues.Add(issue);
                    }
                }

                RaiseIssuesChanged(m_Issues);
            }
        }

        private BatchJobItemStateStatus_e m_Status;
        private readonly List<IBatchJobItemIssue> m_Issues;

        public BatchJobItemStateBase()
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
                RaiseIssuesChanged(m_Issues);
            }
        }

        public void ClearIssues()
        {
            m_Issues.Clear();
            RaiseIssuesChanged(m_Issues);
        }

        protected abstract void RaiseIssuesChanged(IReadOnlyList<IBatchJobItemIssue> issues);
        protected abstract void RaiseStatusChanged(BatchJobItemStateStatus_e status);
    }
}
