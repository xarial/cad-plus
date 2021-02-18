//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus
{
    /// <summary>
    /// Represents stan-alone application
    /// </summary>
    public interface IHost : IDisposable
    {        
        /// <summary>
        /// Accesses the registered modules
        /// </summary>
        IModule[] Modules { get; }

        /// <summary>
        /// Notifies when all modules are initialized
        /// </summary>
        /// <remarks>Use this method to invoke modules APIs</remarks>
        event Action<IApplication> Initialized;

        /// <summary>
        /// Notifies when the application loaded its data and modules can start invoking APIs of this application
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

        /// <summary>
        /// Displays the popup dialog in the current host
        /// </summary>
        /// <typeparam name="TWindow">Window to show in popup</typeparam>
        void ShowPopup<TWindow>(TWindow wnd)
            where TWindow : Window;
    }
}
