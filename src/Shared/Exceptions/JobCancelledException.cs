using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Plus.Shared.Exceptions
{
    public class JobCancelledException : UserException
    {
        public JobCancelledException() : base("Cancelled by the user")
        {
        }
    }
}
