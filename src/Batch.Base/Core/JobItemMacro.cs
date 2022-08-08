//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemMacro : IJobItemOperationMacro
    {
        IJobItemOperationDefinition IJobItemOperation.Definition => Definition;

        public event JobItemOperationStateChangedDelegate StateChanged;
        public event JobItemOperationIssuesChangedDelegate IssuesChanged;
        public event JobItemOperationUserResultChangedDelegate UserResultChanged;

        public Exception InternalMacroException { get; set; }

        public JobItemOperationMacroDefinition Definition { get; }
        
        public JobItemState_e State
        {
            get => m_State;
            set
            {
                m_State = value;
                StateChanged?.Invoke(this, value);
            }
        }

        public IReadOnlyList<IJobItemIssue> Issues => m_Issues;

        public object UserResult { get; }

        private readonly List<IJobItemIssue> m_Issues;
        private JobItemState_e m_State;

        public JobItemMacro(JobItemOperationMacroDefinition macroDef)
        {
            Definition = macroDef;
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
