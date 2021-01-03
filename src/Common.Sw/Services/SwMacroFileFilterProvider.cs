using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Sw.Services
{
    public class SwMacroFileFilterProvider : IMacroFileFilterProvider
    {
        public FileTypeFilter[] GetSupportedMacros()
            => new FileTypeFilter[]
            {
                new FileTypeFilter("VBA Macros", "*.swp"),
                new FileTypeFilter("SWBasic Macros", "*.swb"),
                new FileTypeFilter("VSTA Macros", "*.dll"),
                new FileTypeFilter("All Macros", "*.swp", "*.swb", "*.dll")
            };
    }
}
