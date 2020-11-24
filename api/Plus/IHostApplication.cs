//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus
{
    public delegate void ConfigureServicesDelegate(IXServiceCollection svcColl);

    public interface IHostApplication
    {
        event Action Loaded;
        event ConfigureServicesDelegate ConfigureServices;

        IntPtr ParentWindow { get; }
    }
}
