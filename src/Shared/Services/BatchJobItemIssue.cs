//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchJobItemIssue : IBatchJobItemIssue, IEquatable<IBatchJobItemIssue>
    {
        public BatchJobItemIssueType_e Type { get; }
        public string Content { get; }

        public BatchJobItemIssue(BatchJobItemIssueType_e type, string content)
        {
            Type = type;
            Content = content;
        }

        public bool Equals(IBatchJobItemIssue other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (this.GetType() == other.GetType())
            {
                if (Type == other.Type
                    && string.Equals(Content, other.Content, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
