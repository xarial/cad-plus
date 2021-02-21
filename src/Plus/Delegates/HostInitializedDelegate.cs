using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IHost.Initialized"/> event
    /// </summary>
    /// <param name="app">Host application</param>
    /// <param name="svcProvider">Host services provider</param>
    /// <param name="modules">Modules of this host</param>
    public delegate void HostInitializedDelegate(IApplication app, IServiceContainer svcProvider, IModule[] modules);
}
