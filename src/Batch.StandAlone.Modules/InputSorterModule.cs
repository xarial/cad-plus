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
using Xarial.CadPlus.Batch.StandAlone.Modules.UI;
using Xarial.CadPlus.Batch.StandAlone.Modules.ViewModels;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Attributes;
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

        public void Init(IHost host)
        {
            m_Host = (IHostWpf)host;
            m_Host.Connect += OnConnect;
            m_App = m_Host.Application as IBatchApplication;
        }

        private void OnConnect()
        {
            m_App.ProcessInput += OnProcessInput;
        }

        private void OnProcessInput(IXApplication app, List<string> input)
        {
            var groups = GroupTopological(input, filePath => 
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
            }, StringComparer.CurrentCultureIgnoreCase);

            foreach (var group in groups) 
            {
                foreach (var extraItem in group.Except(input).ToArray()) 
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

            var vm = new InputsSorterVM(items);

            input.Clear();

            m_Host.WpfApplication.Dispatcher.Invoke(() => 
            {
                var wnd = new InputsSorterWindow();
                wnd.DataContext = vm;

                if (wnd.ShowDialog() == true) 
                {
                    foreach (ItemVM item in vm.InputView) 
                    {
                        input.Add(item.FilePath);
                    }
                }
            });
        }

        //function code based on https://www.codeproject.com/Articles/869059/Topological-sorting-in-Csharp with minor modifications
        private List<List<T>> GroupTopological<T>(IEnumerable<T> source,
            Func<T, IEnumerable<T>> getDependenciesFunc, IEqualityComparer<T> comparer = null)
        {
            var sorted = new List<List<T>>();
            var visited = new Dictionary<T, int>(comparer);

            foreach (var item in source)
            {
                Visit(item, getDependenciesFunc, sorted, visited);
            }

            return sorted;
        }

        private int Visit<T>(T item, Func<T, IEnumerable<T>> getDependenciesFunc,
            List<List<T>> sorted, Dictionary<T, int> visited)
        {
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
                        var depLevel = Visit(dependency, getDependenciesFunc, sorted, visited);
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
