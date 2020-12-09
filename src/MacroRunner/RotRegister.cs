//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Xarial.XCad.Toolkit.Windows;

namespace Xarial.CadPlus.MacroRunner
{
    internal class RotRegister : IDisposable
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        private readonly int m_Id;

        internal RotRegister(object obj, string name) 
        {
            m_Id = RotHelper.RegisterComObject(obj, name);
        }
        
        public void Dispose()
        {
            RotHelper.UnregisterComObject(m_Id);
        }
    }
}
