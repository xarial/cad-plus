using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IJobItemDocument : IBatchJobItem
    {
    }

    public interface IJobItemOperationMacro : IBatchJobItemOperation
    {
    }

    public interface IBatchMacroRunJobBase : IBatchJobBase
    {
    }
}
