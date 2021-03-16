using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;

namespace Xarial.CadPlus.Plus.Applications
{
    public interface IHasCommandManager : IApplication
    {
        event CreateCommandManagerDelegate CreateCommandManager;
    }
}
