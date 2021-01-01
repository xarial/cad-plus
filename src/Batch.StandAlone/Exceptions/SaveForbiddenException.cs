using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.XBatch.Base.Exceptions
{
    public class SaveForbiddenException : UserException
    {
        public SaveForbiddenException() : base("Saving is forbidden")
        {
        }
    }
}
