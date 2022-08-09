using System;
using System.Threading;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IBatchJobHandlerServiceFactory
    {
        IBatchJobHandlerService Create(IBatchJob job, string title, CancellationTokenSource cancellationTokenSource);
    }

    public interface IBatchJobHandlerService : IDisposable
    {
    }
}
