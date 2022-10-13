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
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Properties;
using Xarial.CadPlus.Batch.Extensions.Services;
using Xarial.CadPlus.Batch.Extensions.UI;
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.XCad.Extensions;
using Xarial.CadPlus.Plus.Modules.Batch;

namespace Xarial.CadPlus.Batch.Extensions
{
    [Module(typeof(IHostCadExtension))]
    public class InputSorterModuleInApp : IInputSorterModule
    {
        private IHostCadExtension m_Host;
        private IBatchInAppModule m_BatchInAppModule;
        private bool m_EnableOrdering;

        private readonly TopologicalReferencesSorter m_Sorter;

        private ICadDescriptor m_EntDesc;

        public InputSorterModuleInApp()
        {
            m_Sorter = new TopologicalReferencesSorter();
        }

        public void Init(IHost host)
        {
            m_Host = (IHostCadExtension)host;
            m_Host.Initialized += OnInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_BatchInAppModule = modules.OfType<IBatchInAppModule>().FirstOrDefault();

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
                    new RibbonToggleCommand("Order By Dependencies",
                    null, "",
                    () => m_EnableOrdering,
                    x => m_EnableOrdering = x));
            }
        }

        private void OnProcessInput(IXApplication app, List<IXDocument> input)
        {
            if (m_EnableOrdering)
            {
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var src = input.ToArray();
                input.Clear();

                ItemVM[] itemsList;

                using (var prg = app.CreateProgress())
                {
                    prg.SetStatus("Loading dependency tree...");

                    itemsList = m_Sorter.Sort(src,
                                    p => prg.Report(p),
                                    cancellationToken);
                }

                var vm = new InputsSorterVM(m_EntDesc);
                vm.LoadItems(itemsList);

                var popup = m_Host.Extension.CreatePopupWindow<InputsSorterWindow>();
                popup.Control.DataContext = vm;
                
                if (popup.ShowDialog() == true)
                {
                    foreach (ItemVM item in vm.InputView)
                    {
                        input.Add(item.Document);
                    }
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
