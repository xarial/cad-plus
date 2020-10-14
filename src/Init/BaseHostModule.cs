using System;

namespace Xarial.CadPlus.Module.Init
{
    public abstract class BaseHostModule : IHostModule
    {
        public abstract IntPtr ParentWindow { get; }
        public abstract event Action Loaded;

        public BaseHostModule()
        {
            //TODO: implement common initiation logic across multiple apps and add-ins, e.g. initate logger
        }
    }
}
