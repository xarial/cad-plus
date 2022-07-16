//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.MacroRunner
{
    [ComVisible(true)]
    [Guid("77C12C96-9602-4D08-A22C-CB5C280BBDF1")]
    public interface IMacroResult 
    {
        bool Result { get; set; }
        string Message { get; set; }

        void Set(string name, object value);
        object Get(string name);
        bool Exists(string name);
        void Remove(string name);
    }

    [ComVisible(true)]
    [Guid("A72178E3-B5E5-4D04-8C35-0321CAD3C49A")]
    [ProgId("CadPlus.MacroRunner.MacroResult")]
    public class MacroResult : IMacroResult
    {
        private readonly CustomParametersManager m_ParamsMgr;

        public MacroResult()
        {
            m_ParamsMgr = new CustomParametersManager();
        }

        public bool Result { get; set; }
        public string Message { get; set; }

        public object Get(string name) => m_ParamsMgr.Get(name);
        public void Set(string name, object value) => m_ParamsMgr.Set(name, value);
        public bool Exists(string name) => m_ParamsMgr.Exists(name);
        public void Remove(string name) => m_ParamsMgr.Remove(name);
    }
}
