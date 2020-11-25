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
    [Guid("51FD8EF9-505D-40B1-837A-E38A6F2FBA4A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IStatusResult : IMacroResult
    {
        bool Result { get; set; }
        string Message { get; set; }
    }

    [ComVisible(true)]
    [Guid("A72178E3-B5E5-4D04-8C35-0321CAD3C49A")]
    [ProgId("CadPlus.MacroRunner.StatusResult")]
    [ClassInterface(ClassInterfaceType.None)]
    public class StatusResult : IStatusResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
}
