using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Batch
{
    public interface IXCadMacro
    {
        void Run(IXApplication app, IXDocument doc, string[] args);
    }
}
