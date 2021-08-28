using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class PartialCommandGroupAttribute : Attribute
    {
        public int BaseCommandGroupId { get; }

        public PartialCommandGroupAttribute(int baseCommandGroupId) 
        {
            BaseCommandGroupId = baseCommandGroupId;
        }
    }
}
