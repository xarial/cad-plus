using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Sw.Services
{
    public class SwMacroFileFilterProvider : IMacroFileFilterProvider
    {
        public FileFilter[] GetSupportedMacros()
            => new FileFilter[]
            {
                new FileFilter("VBA Macros", "*.swp"),
                new FileFilter("SWBasic Macros", "*.swb"),
                new FileFilter("VSTA Macros", "*.dll"),
                new FileFilter("All Macros", "*.swp", "*.swb", "*.dll"),
                FileFilter.AllFiles
            };
    }
}
