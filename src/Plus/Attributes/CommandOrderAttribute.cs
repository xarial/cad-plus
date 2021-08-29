using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class CommandOrderAttribute : Attribute
    {
        public int Order { get; }

        public CommandOrderAttribute(int order) 
        {
            Order = order;
        }
    }
}
