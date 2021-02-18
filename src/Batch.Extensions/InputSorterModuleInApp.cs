using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Properties;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.Extensions
{
    [Module(typeof(IHostExtension))]
    public class InputSorterModuleInApp : IModule
    {
        private IHost m_Host;
        private IBatchInAppModule m_BatchInAppModule;
        private bool m_EnableOrdering;

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.Initialized += OnInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_BatchInAppModule = modules.OfType<IBatchInAppModule>().FirstOrDefault();

            if (m_BatchInAppModule != null)
            {
                m_BatchInAppModule.ProcessInput += OnProcessInput;
            }
        }

        private void OnConnect()
        {
            if (m_BatchInAppModule != null)
            {
                m_BatchInAppModule.AddCommands(Group_e.Options,
                    new RibbonToggleCommand("Order By Dependencies",
                    null, "",
                    () => m_EnableOrdering,
                    x => m_EnableOrdering = x));
            }
        }

        private void OnProcessInput(IXApplication app, List<IXDocument> input)
        {

        }

        public void Dispose()
        {
        }
    }
}
