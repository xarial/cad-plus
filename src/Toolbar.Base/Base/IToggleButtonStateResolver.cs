//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.XCad;

namespace Xarial.CadPlus.CustomToolbar.Base
{
    public interface IToggleButtonStateResolver
    {
        IXApplication Application { get; }
        bool Resolve();
    }
}
