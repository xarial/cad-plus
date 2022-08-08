//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemIssue : IJobItemIssue, IEquatable<IJobItemIssue>
    {
        public IssueType_e Type { get; }
        public string Content { get; }

        public JobItemIssue(IssueType_e type, string content)
        {
            Type = type;
            Content = content;
        }

        public bool Equals(IJobItemIssue other)
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
