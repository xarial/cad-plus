using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.Base.Exceptions
{
    public class SaveForbiddenException : Exception
    {
        public SaveForbiddenException() : base("Saving is forbidden")
        {
        }
    }
}
