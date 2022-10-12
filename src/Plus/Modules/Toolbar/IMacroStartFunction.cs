//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

namespace Xarial.CadPlus.Plus.Modules.Toolbar
{
    public interface IMacroStartFunction
    {
        string ModuleName { get; }
        string SubName { get; }
    }
}
