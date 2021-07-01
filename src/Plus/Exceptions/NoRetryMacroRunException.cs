using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Exceptions
{
    /// <summary>
    /// Indicates that there should be no retry to run this macro
    /// </summary>
    /// <remarks>Should be used in context of <see cref="IXCadMacro"/></remarks>
    public interface INoRetryMacroRunException
    {
    }

    /// <inheritdoc/>
    public class NoRetryMacroRunException : UserException, INoRetryMacroRunException
    {
        public NoRetryMacroRunException(string userMessage) : base(userMessage)
        {
        }

        public NoRetryMacroRunException(string userMessage, Exception inner) : base(userMessage, inner)
        {
        }
    }
}
