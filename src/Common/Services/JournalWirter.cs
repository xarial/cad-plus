//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Services
{
    public class JournalWriter : TextWriter
    {
        public event Action<string> Log;

        private readonly bool m_AddTimeStamp;

        public JournalWriter(bool addTimeStamp) 
        {
            m_AddTimeStamp = addTimeStamp;
        }

        public override void WriteLine(string value)
        {
            var timeStamp = m_AddTimeStamp ? $" [{DateTime.Now.ToString("hh:mm:ss")}]" : "";

            Log?.Invoke($"{value}{timeStamp}");
        }

        public override Encoding Encoding => Encoding.Default;
    }
}
