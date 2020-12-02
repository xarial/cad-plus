using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Common.Services
{
    public class AppLogger : IXLogger
    {
        private readonly string m_Category;

        public AppLogger()
        {
            m_Category = "CAD+ Toolset";
        }

        public void Log(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg, m_Category);
        }
    }
}
