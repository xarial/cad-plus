using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Services
{
    public interface IMacroFileFilterProvider
    {
        FileFilter[] GetSupportedMacros();
    }
}
