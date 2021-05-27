//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Services
{
    public class AppLogger : IXLogger
    {
        protected readonly string m_Category;

        public AppLogger()
        {
            m_Category = "CAD+ Toolset";
        }
        
        public virtual void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information)
        {
            System.Diagnostics.Trace.WriteLine($"[{severity}]{msg}", m_Category);
        }
    }
}
