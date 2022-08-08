using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IJobItemDocument : IJobItem
    {
    }

    public interface IJobItemOperationMacro : IJobItemOperation
    {
    }

    public interface IBatchMacroRunJobBase : IBatchJobBase
    {
    }
}
