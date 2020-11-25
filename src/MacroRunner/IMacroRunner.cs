//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("35C9ABF1-4C21-4810-B8C1-CB15394A13D1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMacroRunner
    {
        [DispId(1)]
        IMacroResult Run(object appDisp, string macroPath, string moduleName, string subName, int opts, IMacroParameter param, bool cacheReg = false);

        [DispId(2)]
        IMacroParameter PopParameter(object appDisp);
    }
}
