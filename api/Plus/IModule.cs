using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    public interface IModule : IDisposable
    {
        void Init(IHostApplication host);
    }
}
