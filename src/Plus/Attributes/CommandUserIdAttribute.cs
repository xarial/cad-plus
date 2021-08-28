using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandUserIdAttribute : Attribute
    {
        public int UserId { get; }

        public CommandUserIdAttribute(int userId) 
        {
            UserId = userId;
        }
    }
}
