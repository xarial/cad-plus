using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Common.Services
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
