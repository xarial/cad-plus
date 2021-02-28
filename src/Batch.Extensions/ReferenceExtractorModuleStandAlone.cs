using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Models;
using Xarial.CadPlus.Batch.Extensions.Properties;
using Xarial.CadPlus.Batch.Extensions.UI;
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Atributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Batch.Extensions
{
    [Module(typeof(IHostWpf), typeof(IBatchApplication))]
    [ModuleOrder(typeof(IInputSorterModule), ModuleRelativeOrder_e.Before)]
    public class ReferenceExtractorModuleStandAlone : IModule
    {
        private IHostWpf m_Host;
        private IBatchApplication m_App;

        private bool m_ExtractReferences;
        
        public void Init(IHost host)
        {
            m_Host = (IHostWpf)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app, IServiceContainer svcProvider, IModule[] modules)
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

            group.Commands.Add(new RibbonToggleCommand("Extract References",
                Resources.extract_references, "",
                () => m_ExtractReferences,
                x => m_ExtractReferences = x));
        }

        private void OnProcessInput(IXApplication app, ICadApplicationInstanceProvider instProvider, List<IXDocument> input)
        {
            if (m_ExtractReferences)
            {
                var vm = new ReferenceExtractorVM(new ReferenceExtractor(app, instProvider.EntityDescriptor.DrawingFileFilter.Extensions),
                    input.ToArray(), instProvider.EntityDescriptor, ReferencesScope_e.AllReferences, true);
                
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                input.Clear();

                m_Host.WpfApplication.Dispatcher.Invoke(() =>
                {
                    var wnd = new ReferenceExtractorWindow();
                    wnd.DataContext = vm;

                    wnd.Loaded += async (s, e) =>
                    {
                        try
                        {
                            await vm.CollectReferencesAsync();
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    };

                    var res = wnd.ShowDialog();

                    if (res == true)
                    {
                        foreach (ReferenceVM reference in vm.References)
                        {
                            if (reference.IsChecked)
                            {
                                input.Add(reference.Document);
                            }

                            input.AddRange(reference.Drawings.Where(d => d.IsChecked).Select(d => d.Document));
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
