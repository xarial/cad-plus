using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Plus.Services
{
    internal interface IBatchJobReportWriter
    {
        FileFilter Filter { get; }
        void Write(IBatchJobBase job, string filePath, string title);
    }
}
