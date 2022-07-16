using System;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IProgressHandlerFactoryService 
    {
        IProgressHandlerService Create();
    }

    public interface IProgressHandlerService : IDisposable
    {
        void ReportProgress(double prg);
        void SetStatus(string status);
    }
}
