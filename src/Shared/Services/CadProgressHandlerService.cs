using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class CadProgressHandlerFactoryService : IProgressHandlerFactoryService
    {
        private readonly IXApplication m_App;

        public CadProgressHandlerFactoryService(IXApplication app)
        {
            m_App = app;
        }

        public IProgressHandlerService Create() => new CadProgressHandlerService(m_App);
    }

    public class CadProgressHandlerService : IProgressHandlerService
    {
        private readonly IXApplication m_App;
        private readonly IXProgress m_Progress;

        public CadProgressHandlerService(IXApplication app) 
        {
            m_App = app;
            m_Progress = m_App.CreateProgress();
        }

        public void ReportProgress(double prg) => m_Progress.Report(prg);
        public void SetStatus(string status) => m_Progress.SetStatus(status);
        public void Dispose() => m_Progress.Dispose();
    }
}
