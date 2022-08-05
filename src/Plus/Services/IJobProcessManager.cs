using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IJobProcessManager : IDisposable
    {
        void AddProcess(Process process);
    }
}
