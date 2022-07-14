using System;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Services;

namespace Xarial.CadPlus.Batch.StandAlone.Services
{
    public interface IJobContectResilientWorkerFactory 
    {
        IResilientWorker<BatchJobContext> Create(TimeSpan? timeout);
    }
}
