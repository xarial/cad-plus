//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class JobItemMacro : JobItem, IJobItemOperation
    {
        public JobItemMacro(string filePath) : base(filePath)
        {
            DisplayName = Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
