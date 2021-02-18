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
using Xarial.CadPlus.Batch.Extensions.UI;
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.Extensions
{
    internal class DocumentComparer : IEqualityComparer<IXDocument>
    {
        public bool Equals(IXDocument x, IXDocument y)
            => string.Equals(x.Path, y.Path, StringComparison.CurrentCultureIgnoreCase);

        public int GetHashCode(IXDocument obj)
            => 0;
    }

    [Module(typeof(IHostWpf), typeof(IBatchApplication))]
    public class InputSorterModuleStandAlone : IModule
    {
        private IHostWpf m_Host;
        private IBatchApplication m_App;

        private bool m_EnableOrdering;

        public void Init(IHost host)
        {
            m_Host = (IHostWpf)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app)
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
                Resources.order_dependencies, "",
                () => m_EnableOrdering,
                x => m_EnableOrdering = x));
        }

        private void OnProcessInput(IXApplication app, List<IXDocument> input)
        {
            if (m_EnableOrdering)
            {
                var vm = new InputsSorterVM();

                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var src = input.ToArray();
                input.Clear();

                m_Host.WpfApplication.Dispatcher.Invoke(() =>
                {
                    var wnd = new InputsSorterWindow();
                    wnd.DataContext = vm;

                    wnd.Loaded += async (s, e)=> 
                    {
                        try
                        {
                            var itemsList = await Task.Run(() =>
                            {
                                var groups = GroupTopological(src, doc =>
                                {
                                    try
                                    {
                                        return doc.Dependencies;
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                }, new DocumentComparer(),
                                p => vm.Progress = p,
                                cancellationToken);

                                foreach (var group in groups)
                                {
                                    foreach (var extraItem in group.Except(src).ToArray())
                                    {
                                        group.Remove(extraItem);
                                    }
                                }

                                var items = new List<ItemVM>();

                                for (int i = 0; i < groups.Count; i++)
                                {
                                    items.AddRange(groups[i].Select(doc => new ItemVM()
                                    {
                                        Document = doc,
                                        Level = groups.Count - i - 1
                                    }));
                                }

                                return items;
                            });

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
        
        //function code based on https://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp with minor modifications
        private List<List<T>> GroupTopological<T>(T[] source,
            Func<T, IEnumerable<T>> getDependenciesFunc, IEqualityComparer<T> comparer,
            Action<double> prgHandler,
            CancellationToken cancellationToken)
        {
            var sorted = new List<List<T>>();
            var visited = new Dictionary<T, int>(comparer);

            int curInst = 0;

            foreach (var item in source)
            {
                Visit(item, getDependenciesFunc, sorted, visited, cancellationToken);
                prgHandler.Invoke((double)++curInst / (double)source.Length);
            }

            return sorted;
        }

        private int Visit<T>(T item, Func<T, IEnumerable<T>> getDependenciesFunc,
            List<List<T>> sorted, Dictionary<T, int> visited, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const int curLevel = -1;
            int level;
            var hasVisited = visited.TryGetValue(item, out level);

            if (hasVisited)
            {
                if (level == curLevel)
                {
                    //throw new ArgumentException("Cyclic dependency found.");
                }
            }
            else
            {
                level = curLevel;
                visited[item] = level;

                var dependencies = getDependenciesFunc.Invoke(item);

                if (dependencies?.Any() == true)
                {
                    foreach (var dependency in dependencies)
                    {
                        var depLevel = Visit(dependency, getDependenciesFunc, sorted, visited, cancellationToken);
                        level = Math.Max(level, depLevel);
                    }
                }

                visited[item] = ++level;

                while (sorted.Count <= level)
                {
                    sorted.Add(new List<T>());
                }

                sorted[level].Add(item);
            }

            return level;
        }

        public void Dispose()
        {
        }
    }
}
