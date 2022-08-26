//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    /// <summary>
    /// Represents the extension module for this host application
    /// </summary>
    public interface IModule : IDisposable
    {
        /// <summary>
        /// Called when instance of the module is created
        /// </summary>
        /// <remarks>Only subscribe to events and implement the actual loading from <see cref="IHost.Connect"/></remarks>
        /// <param name="host">Current host</param>
        void Init(IHost host);
    }
}
