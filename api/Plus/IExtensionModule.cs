//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Plus
{
    public interface IExtensionModule : IDisposable
    {
        void Init(IHostExtensionApplication host);
    }
}
