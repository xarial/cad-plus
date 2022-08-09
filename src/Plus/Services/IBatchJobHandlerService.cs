using System;
using System.Threading;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IBatchJobHandlerServiceFactory
    {
        IBatchJobHandlerService Create(IBatchJob job, CancellationTokenSource cancellationTokenSource);
    }

    public interface IBatchJobHandlerService : IDisposable
    {
    }
}
