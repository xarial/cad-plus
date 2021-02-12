using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Delegates
{
    public delegate void ProcessBatchInputDelegate(IXApplication app, List<IXDocument> input);
}
