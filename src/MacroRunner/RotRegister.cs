//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Windows;

namespace Xarial.CadPlus.MacroRunner
{
    internal class RotRegister : IDisposable
    {
        private readonly int m_Id;

        private IXLogger m_Logger;

        internal RotRegister(object obj, string name, IXLogger logger) 
        {
            m_Logger = logger;
            m_Id = RotHelper.RegisterComObject(obj, name, true, false, m_Logger);
        }
        
        public void Dispose()
        {
            m_Logger.Log($"Unregistering object from ROT: {m_Id}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
            RotHelper.UnregisterComObject(m_Id);
        }
    }
}
