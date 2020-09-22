using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Services
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
