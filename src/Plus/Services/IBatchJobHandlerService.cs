//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        event Action<IBatchJobHandlerService> Disposed;
        
        string Title { get; }
        void Run();
    }
}
