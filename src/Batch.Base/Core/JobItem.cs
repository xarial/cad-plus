//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItem : IJobItem
    {
        public event Action<IJobItem, JobItemStatus_e> StatusChanged;
        public event Action<IJobItem> IssuesChanged;

        public string DisplayName { get; protected set; }
        
        public string FilePath { get; }

        public JobItemStatus_e Status 
        {
            get => m_Status;
            set 
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        private List<string> m_Issues;

        public IReadOnlyList<string> Issues => m_Issues;

        private JobItemStatus_e m_Status;

        internal JobItem(string filePath) 
        {
            FilePath = filePath;
            DisplayName = filePath;
            m_Status = JobItemStatus_e.AwaitingProcessing;
            m_Issues = new List<string>();
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

            ReportIssue(err);
        }

        public void ReportIssue(string issue)
        {
            if (!m_Issues.Contains(issue))
            {
                m_Issues.Add(issue);
                IssuesChanged?.Invoke(this);
            }
        }

        public void ClearIssues()
        {
            m_Issues.Clear();
            IssuesChanged?.Invoke(this);
        }
    }
}
