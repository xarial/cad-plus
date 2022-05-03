//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.StandAlone
{
    public class BatchApplication : IBatchApplication
    {
        public event ProcessBatchInputDelegate ProcessInput;
        public event CreateCommandManagerDelegate CreateCommandManager;

        public ICadApplicationInstanceProvider[] ApplicationProviders => m_ApplicationProviders.ToArray();

        private readonly List<ICadApplicationInstanceProvider> m_ApplicationProviders;

        private readonly IBatchApplicationProxy m_Proxy;

        public BatchApplication(IBatchApplicationProxy proxy)
        {
            m_Proxy = proxy;
            m_Proxy.RequestProcessInput += OnRequestProcessInput;
            m_Proxy.RequestCreateCommandManager += OnRequestCreateCommandManager;
            m_ApplicationProviders = new List<ICadApplicationInstanceProvider>();
        }

        private void OnRequestCreateCommandManager(IRibbonCommandManager cmdMgr)
            => CreateCommandManager?.Invoke(cmdMgr);

        private void OnRequestProcessInput(IXApplication app, ICadApplicationInstanceProvider instProvider, List<IXDocument> input)
            => ProcessInput?.Invoke(app, instProvider, input);

        public void RegisterApplicationProvider(ICadApplicationInstanceProvider provider)
            => m_ApplicationProviders.Add(provider);
    }

    public interface IBatchApplicationProxy
    {
        event ProcessBatchInputDelegate RequestProcessInput;
        event Action<IRibbonCommandManager> RequestCreateCommandManager;

        void ProcessInput(IXApplication app, ICadApplicationInstanceProvider instProvider, List<IXDocument> input);
        void CreateCommandManager(IRibbonCommandManager cmdMgr);
    }

    internal class BatchApplicationProxy : IBatchApplicationProxy
    {
        public event ProcessBatchInputDelegate RequestProcessInput;
        public event Action<IRibbonCommandManager> RequestCreateCommandManager;

        public void CreateCommandManager(IRibbonCommandManager cmdMgr)
            => RequestCreateCommandManager?.Invoke(cmdMgr);

        public void ProcessInput(IXApplication app, ICadApplicationInstanceProvider instProvider, List<IXDocument> input)
            => RequestProcessInput?.Invoke(app, instProvider, input);
    }
}
