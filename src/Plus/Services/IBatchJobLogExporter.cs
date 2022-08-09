using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IBatchJobLogExporter
    {
        FileFilter Filter { get; }
        void Export(IBatchJobBase job, string filePath);
    }
}
