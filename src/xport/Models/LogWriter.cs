//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Text;

namespace Xarial.CadPlus.Xport.Models
{
    public class LogWriter : TextWriter
    {
        public event Action<string> Log;

        public override void WriteLine(string value)
        {
            Log?.Invoke(value);
        }

        public override Encoding Encoding => Encoding.Default;
    }
}