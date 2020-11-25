using System;
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.Module.Init
{
    public abstract class BaseHostApplication : IHostApplication
    {
        public abstract IntPtr ParentWindow { get; }
        
        public virtual event ConfigureServicesDelegate ConfigureServices;
        public virtual event Action Loaded;

        public BaseHostApplication()
        {
            //TODO: implement common initiation logic across multiple apps and add-ins, e.g. initate logger
        }
    }
}
