using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IDocumentProvider
    {
        string[] SupportedFileExtensions { get; }
        IXDocument GetDocument(string path);
    }
}
