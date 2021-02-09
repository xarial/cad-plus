//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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

namespace Xarial.CadPlus.Batch.StandAlone
{
    internal class BatchApplication : IBatchApplication
    {
        public event ProcessInputDelegate ProcessInput;
        public event CreateCommandManagerDelegate CreateCommandManager;

        public Guid Id => Guid.Parse(ApplicationIds.BatchStandAlone);

        public IApplicationProvider[] ApplicationProviders => m_ApplicationProviders.ToArray();

        private readonly List<IApplicationProvider> m_ApplicationProviders;

        private readonly IBatchApplicationProxy m_Proxy;

        internal BatchApplication(IBatchApplicationProxy proxy)
        {
            m_Proxy = proxy;
            m_Proxy.RequestProcessInput += OnRequestProcessInput;
            m_Proxy.RequestCreateCommandManager += OnRequestCreateCommandManager;
            m_ApplicationProviders = new List<IApplicationProvider>();
        }

        private void OnRequestCreateCommandManager(IRibbonCommandManager cmdMgr)
            => CreateCommandManager?.Invoke(cmdMgr);

        private void OnRequestProcessInput(IXApplication app, List<string> input)
            => ProcessInput?.Invoke(app, input);

        public void RegisterApplicationProvider(IApplicationProvider provider)
            => m_ApplicationProviders.Add(provider);
    }

    public interface IBatchApplicationProxy
    {
        event Action<IXApplication, List<string>> RequestProcessInput;
        event Action<IRibbonCommandManager> RequestCreateCommandManager;

        void ProcessInput(IXApplication app, List<string> input);
        void CreateCommandManager(IRibbonCommandManager cmdMgr);
    }

    internal class BatchApplicationProxy : IBatchApplicationProxy
    {
        public event Action<IXApplication, List<string>> RequestProcessInput;
        public event Action<IRibbonCommandManager> RequestCreateCommandManager;

        public void CreateCommandManager(IRibbonCommandManager cmdMgr)
            => RequestCreateCommandManager?.Invoke(cmdMgr);

        public void ProcessInput(IXApplication app, List<string> input)
            => RequestProcessInput?.Invoke(app, input);
    }
}
