//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

namespace Xarial.CadPlus.MacroRunner
{
    internal class CustomParametersManager 
    {
        private readonly Dictionary<string, object> m_Parameters;

        internal CustomParametersManager() 
        {
            m_Parameters = new Dictionary<string, object>(
                StringComparer.CurrentCultureIgnoreCase);
        }

        internal void Set(string name, object value) => m_Parameters[name] = value;
        internal object Get(string name) => m_Parameters[name];
        internal bool Exists(string name) => m_Parameters.ContainsKey(name);
        internal void Remove(string name) => m_Parameters.Remove(name);
    }
}
