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
    [Guid("B2C8F829-9E75-4587-A1A2-CD3C02BA2CB9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMacroParameter
    {
        [DispId(1)]
        IMacroResult Result { get; set; }
    }
}
