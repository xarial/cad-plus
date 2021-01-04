using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IDocumentAdapter
    {
        IXDocument GetAdapter(IXDocument doc);
        void DisposeDocument(IXDocument doc);
        void ApplyChanged(IXDocument doc);
    }
}
