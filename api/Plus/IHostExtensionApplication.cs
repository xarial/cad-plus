//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.CadPlus.Plus
{
    public delegate void CommandHandler<TCmd>(TCmd cmd) where TCmd : Enum;

    public interface IHostExtensionApplication : IHostApplication
    {
        event Action Connect;
        event Action Disconnect;

        IXExtension Extension { get; }

        void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum;

        IXPropertyPage<TData> CreatePage<TData>();
    }
}
