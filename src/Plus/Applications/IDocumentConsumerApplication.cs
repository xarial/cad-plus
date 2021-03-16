using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Applications
{
    public interface IDocumentConsumerApplication : IApplication
    {
        IDocumentProvider[] DocumentProviders { get; }
        void RegisterDocumentProvider(IDocumentProvider provider);
    }
}
