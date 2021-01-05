using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Applications
{
    public interface IBomApplication : IApplication
    {
        void ViewBom(string filePath);
        IDocumentProvider[] DocumentProviders { get; }
        void RegisterDocumentProvider(IDocumentProvider provider);
    }
}
