//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
            var groups = GroupTopological(src, prgHandler, cancellationToken);

            var items = new List<ItemVM>();

            var curLevel = 0;

            for (int i = groups.Count - 1; i >= 0 ; i--)
            {
                var docs = groups[i].Intersect(src, new DocumentComparer());

                if (docs.Any())
                {
                    items.AddRange(docs.Select(doc => new ItemVM()
                    {
                        Document = doc,
                        Level = curLevel
                    }));

                    curLevel++;
                }
            }

            return items.ToArray();
        }

        private List<List<IXDocument>> GroupTopological(IXDocument[] docs, Action<double> prgHandler,
            CancellationToken cancellationToken)
        {
            var grouped = new List<List<IXDocument>>();
            var processed = new Dictionary<IXDocument, int>(new DocumentComparer());

            for (int i = 0; i < docs.Length; i++)
            {
                var doc = docs[i];

                Process(doc, grouped, processed, cancellationToken);

                prgHandler.Invoke((double)(i + 1) / (double)docs.Length);
            }

            return grouped;
        }

        private int Process(IXDocument doc,
            List<List<IXDocument>> grouped, Dictionary<IXDocument, int> processed,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!processed.TryGetValue(doc, out int level))
            {
                level = -1;
                processed[doc] = level;

                try
                {
                    var dependencies = doc.Dependencies;

                    if (dependencies?.Any() == true)
                    {
                        foreach (var dependency in dependencies)
                        {
                            var depLevel = Process(dependency, grouped, processed, cancellationToken);
                            level = Math.Max(level, depLevel);
                        }
                    }
                }
                catch 
                {
                }

                processed[doc] = ++level;

                if (level >= grouped.Count) 
                {
                    grouped.AddRange(Enumerable.Repeat(new List<IXDocument>(), 
                        level - grouped.Count + 1));
                }
                
                grouped[level].Add(doc);
            }

            return level;
        }
    }
}
