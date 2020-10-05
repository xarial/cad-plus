using Xarial.XCad;

namespace Xarial.CadPlus.ExtensionModule
{
    public interface IToggleBuggonStateResolver
    {
        IXApplication Application { get; }
        bool Resolve();
    }
}
