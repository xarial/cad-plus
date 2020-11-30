//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus
{
    /// <summary>
    /// Represents stan-alone application
    /// </summary>
    public interface IHostApplication : IDisposable
    {
        /// <summary>
        /// Accesses the registered modules
        /// </summary>
        IEnumerable<IModule> Modules { get; }

        /// <summary>
        /// Notifies when the application loaded its data and modules can start invoking APIs
        /// </summary>
        event Action Connect;

        /// <summary>
        /// Notifies when application closes for modules to release resources
        /// </summary>
        event Action Disconnect;

        /// <summary>
        /// Invoked when application loaded its UI
        /// </summary>
        /// <remarks>Use this to display any popup windows, such as license or registration/login dialogs</remarks>
        void OnStarted();
        
        /// <summary>
        /// Provides an access to services registered in this host
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Parent window of this application
        /// </summary>
        IntPtr ParentWindow { get; }
    }
}
