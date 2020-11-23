using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Extensions;

namespace Xarial.CadPlus.Plus
{
    public delegate void ConfigureServicesDelegate(IXServiceCollection svcColl);
    public delegate void CommandHandler<TCmd>(TCmd cmd) where TCmd : Enum;

    public interface IHostExtensionApplication : IHostApplication
    {
        event Action Connect;
        event Action Disconnect;
        event ConfigureServicesDelegate ConfigureServices;

        IXExtension Extension { get; }

        void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum;
    }
}
