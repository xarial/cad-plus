using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Plus.Shared.Exceptions
{
    public class NotXCadMacroDllException : UserException
    {
        public NotXCadMacroDllException() : base("No xCAD macros found in the specified dll")
        {
        }
    }
}
