using System;
using System.Threading;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IProgressHandlerFactoryService 
    {
        IProgressHandlerService Create(CancellationTokenSource cancellationTokenSource);
    }

    public interface IProgressHandlerService : IDisposable
    {
        void ReportProgress(double prg);
        void SetStatus(string status);
    }
}
