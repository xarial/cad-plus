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
        IMacroResult Result { get; set; }
        void Set(string name, object value);
        object Get(string name);
        bool Exists(string name);
        void Remove(string name);
    }

    [ComVisible(true)]
    [Guid("860F96F7-4BC4-4927-AC8A-2C6176C7C0CB")]
    [ProgId("CadPlus.MacroRunner.MacroParameter")]
    public class MacroParameter : IMacroParameter
    {
        public IMacroResult Result { get; set; }

        private readonly CustomParametersManager m_ParamsMgr;

        public MacroParameter() 
        {
            m_ParamsMgr = new CustomParametersManager();
            Result = new MacroResult()
            {
                Result = true
            };
        }

        public object Get(string name) => m_ParamsMgr.Get(name);
        public void Set(string name, object value) => m_ParamsMgr.Set(name, value);
        public bool Exists(string name) => m_ParamsMgr.Exists(name);
        public void Remove(string name) => m_ParamsMgr.Remove(name);
    }
}
