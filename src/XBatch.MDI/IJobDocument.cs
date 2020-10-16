using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.MDI
{
    public interface IJobDocument
    {
        string Name { get; }
        IJobSettings Settings { get; }
        IEnumerable<IJobResult> Results { get; }
    }
}
