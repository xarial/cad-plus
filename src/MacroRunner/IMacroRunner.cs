//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("35C9ABF1-4C21-4810-B8C1-CB15394A13D1")]
    public interface IMacroRunner
    {
        IMacroResult Run(object appDisp, string macroPath, string moduleName, string subName, int opts, IMacroParameter param, bool cacheReg = false);

        IMacroParameter PopParameter(object appDisp);

        void Dispose();
    }
}
