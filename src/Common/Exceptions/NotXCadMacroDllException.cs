using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Exceptions
{
    public class NotXCadMacroDllException : Exception
    {
        public NotXCadMacroDllException() : base("No xCAD macros found in the specified dll") 
        {
        }
    }
}
