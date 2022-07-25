//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.CadPlus.Plus
{
    public delegate void CommandHandler<TCmd>(TCmd cmd) where TCmd : Enum;

    /// <summary>
    /// Represents specific host application which runs inside the CAD host process
    /// </summary>
    public interface IHostCadExtension : IHost
    {
        /// <summary>
        /// Pointer to the extension add-in
        /// </summary>
        IXExtension Extension { get; }

        /// <summary>
        /// Registers new command in the extension
        /// </summary>
        /// <typeparam name="TCmd">Commands spec</typeparam>
        /// <param name="handler">Command handler</param>
        void RegisterCommands<TCmd>(CommandHandler<TCmd> handler)
            where TCmd : Enum;
    }
}
