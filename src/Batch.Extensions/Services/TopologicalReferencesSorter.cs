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
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.Extensions.Services
{
    public class TopologicalReferencesSorter
    {
        public ItemVM[] Sort(IXDocument[] src, Action<double> prgHandler, CancellationToken cancellationToken) 
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
                prgHandler,
                cancellationToken);

            var items = new List<ItemVM>();

            for (int i = 0; i < groups.Count; i++)
            {
                items.AddRange(groups[i].Select(doc => new ItemVM()
                {
                    Document = doc,
                    Level = groups.Count - i - 1
                }));
            }

            return items.ToArray();
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
    }
}
