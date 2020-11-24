//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
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
