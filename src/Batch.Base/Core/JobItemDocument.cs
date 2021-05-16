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

namespace Xarial.CadPlus.Batch.Base.Core
{
    public class JobItemDocument : JobItem, IJobItemDocument
    {
        public IXDocument Document { get; }

        public JobItemDocument(IXDocument doc, JobItemMacro[] macros) : base(doc.Path)
        {
            Document = doc;
            DisplayName = Path.GetFileName(doc.Path);
            Macros = macros;
        }

        public IEnumerable<IJobItemOperation> Operations => Macros;

        public JobItemMacro[] Macros { get; }
    }
}
