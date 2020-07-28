using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Extensions;

namespace Xarial.CadPlus.ExtensionModule
{
    public interface IModule : IDisposable
    {
        void Load(IXExtension ext);
    }
}
