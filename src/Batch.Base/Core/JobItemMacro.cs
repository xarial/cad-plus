//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemMacro : JobItem, IJobItemOperation
    {
        public MacroData Macro { get; }
        
        public JobItemMacro(MacroData macro) : base(macro.FilePath)
        {
            Macro = macro;
            DisplayName = Path.GetFileNameWithoutExtension(macro.FilePath);
        }
    }
}
