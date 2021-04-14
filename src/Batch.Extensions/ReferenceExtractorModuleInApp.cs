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
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Properties;
using Xarial.CadPlus.Batch.Extensions.Services;
using Xarial.CadPlus.Batch.Extensions.UI;
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Atributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Batch.Extensions
{
    [Module(typeof(IHostExtension))]
    [ModuleOrder(typeof(IInputSorterModule), ModuleRelativeOrder_e.Before)]
    public class ReferenceExtractorModuleInApp : IModule
    {
        private IHostExtension m_Host;
        private IBatchInAppModule m_BatchInAppModule;
        private bool m_FindDrawings;
        private ICadDescriptor m_EntDesc;
        private IXLogger m_Logger;
        private IMessageService m_MsgSvc;

        public void Init(IHost host)
        {
            m_Host = (IHostExtension)host;
            m_Host.Initialized += OnInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnInitialized(IApplication app, IServiceContainer svcProvider, IModule[] modules)
        {
            m_BatchInAppModule = modules.OfType<IBatchInAppModule>().FirstOrDefault();

            m_Logger = svcProvider.GetService<IXLogger>();
            m_MsgSvc = svcProvider.GetService<IMessageService>();

            if (m_BatchInAppModule != null)
            {
                m_EntDesc = svcProvider.GetService<ICadDescriptor>();
                m_BatchInAppModule.ProcessInput += OnProcessInput;
            }
        }

        private void OnConnect()
        {
            if (m_BatchInAppModule != null)
            {
                m_BatchInAppModule.AddCommands(BatchModuleGroup_e.Options,
                    new RibbonToggleCommand("Find Drawings",
                    null, "",
                    () => m_FindDrawings,
                    x => m_FindDrawings = x));
            }
        }

        private void OnProcessInput(IXApplication app, List<IXDocument> input)
        {
            if (m_FindDrawings)
            {
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                
                var vm = new ReferenceExtractorVM(new ReferenceExtractor(app, m_EntDesc.DrawingFileFilter.Extensions),
                    input.ToArray(), m_EntDesc, m_Logger, m_MsgSvc, ReferencesScope_e.SourceDocumentsOnly, true, cancellationToken);

                input.Clear();

                var popup = m_Host.Extension.CreatePopupWindow<ReferenceExtractorWindow>();
                popup.Control.DataContext = vm;

                popup.Control.Loaded += async (s, e) =>
                {
                    try
                    {
                        await vm.CollectReferencesAsync();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                };

                if (popup.ShowDialog() == true)
                {
                    input.AddRange(vm.GetCheckedDocuments());
                }
                else
                {
                    cts.Cancel();
                    throw new OperationCanceledException();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
