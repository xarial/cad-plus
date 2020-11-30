using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Exceptions
{
    public class MacroRunnerResultError : UserException
    {
        public MacroRunnerResultError(string msg) : base($"Macro returns an error: {msg}")
        {
        }
    }
}
