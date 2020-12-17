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

    /// <summary>
    /// Represents specific hsot application which runs inside the CAD host process
    /// </summary>
    public interface IHostExtensionApplication : IHostApplication
    {
        /// <summary>
        /// Pointer to the extension add0in
        /// </summary>
        IXExtension Extension { get; }

        /// <summary>
        /// Registers new command in teh extension
        /// </summary>
        /// <typeparam name="TCmd">Commands spec</typeparam>
        /// <param name="handler">Command handler</param>
        void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum;

        /// <summary>
        /// Creates extension-specific page from the data
        /// </summary>
        /// <typeparam name="TData">Page data specification</typeparam>
        /// <returns>Created page</returns>
        IXPropertyPage<TData> CreatePage<TData>();
    }
}
