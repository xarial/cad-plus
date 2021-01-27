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
using Xarial.XCad;

namespace Xarial.CadPlus.Batch.StandAlone
{
    internal class BatchApplication : IBatchApplication
    {
        public event ProcessInputDelegate ProcessInput;

        public Guid Id => Guid.Parse(ApplicationIds.BatchStandAlone);

        public IApplicationProvider[] ApplicationProviders => m_ApplicationProviders.ToArray();

        private readonly List<IApplicationProvider> m_ApplicationProviders;

        internal IBatchApplicationProxy Proxy { get; }

        internal BatchApplication(IBatchApplicationProxy proxy)
        {
            Proxy = proxy;
            Proxy.RequestProcessInput += OnRequestProcessInput;
            m_ApplicationProviders = new List<IApplicationProvider>();
        }

        private void OnRequestProcessInput(IXApplication app, List<string> input)
            => ProcessInput?.Invoke(app, input);

        public void RegisterApplicationProvider(IApplicationProvider provider)
            => m_ApplicationProviders.Add(provider);
    }

    public interface IBatchApplicationProxy
    {
        event Action<IXApplication, List<string>> RequestProcessInput;
        void ProcessInput(IXApplication app, List<string> input);
    }

    internal class BatchApplicationProxy : IBatchApplicationProxy
    {
        public event Action<IXApplication, List<string>> RequestProcessInput;

        public void ProcessInput(IXApplication app, List<string> input)
            => RequestProcessInput?.Invoke(app, input);
    }
}
