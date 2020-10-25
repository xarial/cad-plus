using System;

namespace Xarial.CadPlus.Module.Init
{
    //TODO: move to Module dll
    public interface IHostModule 
    {
        event Action Loaded;
        IntPtr ParentWindow { get; }
    }
}
