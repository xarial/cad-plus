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
        /// Permanend identifier of this host
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Accesses the registered modules
        /// </summary>
        IEnumerable<IModule> Modules { get; }

        /// <summary>
        /// Notifies when all modules are initialized
        /// </summary>
        /// <remarks>Use this method to invoke modules APIs</remarks>
        event Action Initialized;

        /// <summary>
        /// Notifies when the application loaded its data and modules can start invoking APIs
        /// </summary>
        event Action Connect;

        /// <summary>
        /// Allows to inject services to dependency injection container
        /// </summary>
        event Action<IContainerBuilder> ConfigureServices;

        /// <summary>
        /// Notifies when application closes for modules to release resources
        /// </summary>
        event Action Disconnect;

        /// <summary>
        /// Invoked when application loaded its UI
        /// </summary>
        /// <remarks>Use this to display any popup windows, such as license or registration/login dialogs</remarks>
        event Action Started;
        
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
