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
using Xarial.CadPlus.Batch.StandAlone.Modules.Properties;
using Xarial.CadPlus.Batch.StandAlone.Modules.UI;
using Xarial.CadPlus.Batch.StandAlone.Modules.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.StandAlone.Modules
{
    [Module(typeof(IHostWpf), ApplicationIds.BatchStandAlone)]
    public class InputSorterModule : IModule
    {
        public Guid Id => Guid.Parse("FBC79A8D-79F9-4939-9058-86DD5015A370");

        private IHostWpf m_Host;
        private IBatchApplication m_App;

        private bool m_EnableOrdering;

        public void Init(IHost host)
        {
            m_Host = (IHostWpf)host;
            m_Host.Connect += OnConnect;
            m_App = m_Host.Application as IBatchApplication;
        }

        private void OnConnect()
        {
            m_App.ProcessInput += OnProcessInput;
            m_App.CreateCommandManager += OnCreateCommandManager;
        }

        private void OnCreateCommandManager(IRibbonCommandManager cmdMgr)
        {
            var execGroup = cmdMgr.Tabs.First(t => string.Equals(t.Name, BatchApplicationCommandManager.JobTab.Name))
                .Groups.First(g => string.Equals(g.Name, BatchApplicationCommandManager.JobTab.ExecutionGroupName));

            execGroup.Commands.Add(new RibbonToggleCommand("Order By Dependencies",
                Resources.order_dependencies,
                () => m_EnableOrdering,
                x => m_EnableOrdering = x));
        }

        private void OnProcessInput(IXApplication app, List<string> input)
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
                                var groups = GroupTopological(src, filePath =>
                                {
                                    var doc = app.Documents.PreCreate<IXDocument>();
                                    doc.Path = filePath;

                                    try
                                    {
                                        return doc.Dependencies.Select(d => d.Path);
                                    }
                                    catch
                                    {
                                        return null;
                                    }
                                }, StringComparer.CurrentCultureIgnoreCase,
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
                                    items.AddRange(groups[i].Select(f => new ItemVM()
                                    {
                                        FilePath = f,
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
                            input.Add(item.FilePath);
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
