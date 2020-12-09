//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.IO;
using Xarial.CadPlus.Common.Services;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class JobItemFile : JobItem, IJobItemFile
    {
        public JobItemFile(string filePath, JobItemMacro[] macros) : base(filePath)
        {
            DisplayName = Path.GetFileName(filePath);
            Macros = macros;
        }

        IEnumerable<IJobItemOperation> IJobItemFile.Operations => Macros;

        public JobItemMacro[] Macros { get; }
    }
}
