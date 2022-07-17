using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IXCadMacroProvider
    {
        IXCadMacro GetMacro(string path);
    }
}
