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
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.Extensions
{
    [Module(typeof(IHostWpf), typeof(IBatchApplication))]
    public class InputSorterModuleStandAlone : IInputSorterModule
    {
        private IHostWpf m_Host;
        private IBatchApplication m_App;

        private bool m_EnableOrdering;

        private readonly TopologicalReferencesSorter m_Sorter;

        public InputSorterModuleStandAlone() 
        {
            m_Sorter = new TopologicalReferencesSorter();
        }

        public void Init(IHost host)
        {
            m_Host = (IHostWpf)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_App = (IBatchApplication)app;
        }

        private void OnConnect()
        {
            m_App.ProcessInput += OnProcessInput;
            m_App.CreateCommandManager += OnCreateCommandManager;
        }

        private void OnCreateCommandManager(IRibbonCommandManager cmdMgr)
        {
            if (!cmdMgr.TryGetTab(BatchApplicationCommandManager.InputTab.Name, out IRibbonTab inputTab)) 
            {
                inputTab = new RibbonTab(BatchApplicationCommandManager.InputTab.Name, "Input");
                cmdMgr.Tabs.Add(inputTab);
            }

            if (!inputTab.TryGetGroup("References", out IRibbonGroup group))
            {
                group = new RibbonGroup("References", "References");
                inputTab.Groups.Add(group);
            }

            group.Commands.Add(new RibbonToggleCommand("Order By Dependencies",
                Resources.order_dependencies, "Order input files based on the hierarchical dependency",
                () => m_EnableOrdering,
                x => m_EnableOrdering = x));
        }

        private void OnProcessInput(IXApplication app, ICadApplicationInstanceProvider instProvider, List<IXDocument> input)
        {
            if (m_EnableOrdering)
            {
                var vm = new InputsSorterVM(instProvider.Descriptor);
                
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var src = input.ToArray();
                input.Clear();

                m_Host.WpfApplication.Dispatcher.Invoke(() =>
                {
                    var wnd = new InputsSorterWindow();
                    wnd.Owner = m_Host.WpfApplication.MainWindow;
                    wnd.DataContext = vm;

                    wnd.Loaded += async (s, e)=> 
                    {
                        try
                        {
                            var itemsList = await Task.Run(
                                () => m_Sorter.Sort(src,
                                p => vm.Progress = p,
                                cancellationToken));

                            vm.LoadItems(itemsList);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    };

                    var res = wnd.ShowDialog();
                    
                    if (res == true)
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
                });
            }
        }
        
        public void Dispose()
        {
        }
    }
}
