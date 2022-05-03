//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Services;
using Xarial.CadPlus.Batch.Extensions.ViewModels;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Extensions;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Batch.Extensions.Services
{
    public class ReferenceExtractor
    {
        private readonly string[] m_DrwExtensions;

        private readonly IXApplication m_App;

        public ReferenceExtractor(IXApplication app, string[] drwExtensions) 
        {
            m_App = app;
            m_DrwExtensions = drwExtensions;
        }

        public IXDocument[] GetAllReferences(IXDocument[] docs, ReferencesScope_e scope) 
        {
            if (scope == ReferencesScope_e.SourceDocumentsOnly) 
            {
                return docs;
            }
            
            var allRefs = docs.ToDictionary(
                d => d.Path,
                d => d,
                StringComparer.CurrentCultureIgnoreCase);

            foreach (var doc in docs) 
            {
                IXDocument3D[] deps = null;

                switch (scope) 
                {
                    case ReferencesScope_e.TopLevelDependencies:
                        deps = doc.IterateDependencies(true).ToArray();
                        break;

                    case ReferencesScope_e.AllDependencies:
                        deps = doc.IterateDependencies(false).ToArray();
                        break;

                    default:
                        throw new NotSupportedException();
                }

                foreach (var dep in deps) 
                {
                    if (!allRefs.ContainsKey(dep.Path)) 
                    {
                        allRefs.Add(dep.Path, dep);
                    }
                }
            }

            return allRefs.Values.ToArray();
        }

        public Dictionary<IXDocument, IXDrawing[]> FindAllDrawings(
            IXDocument[] docs, string[] additionalFolders, Action<double> prgHandler, CancellationToken cancellationToken)
        {
            var workDirs = GetDocumentsWorkingDirectories(docs);

            var res = new Dictionary<IXDocument, IXDrawing[]>(new DocumentComparer());

            var searchDrawings = new List<string>();

            foreach (var searchDir in FileSystemUtils.GetTopFolders(
                workDirs.Union(additionalFolders ?? new string[0])))
            {
                foreach (var filter in m_DrwExtensions)
                {
                    if (Directory.Exists(searchDir))
                    {
                        searchDrawings.AddRange(TryGetAllFiles(searchDir, filter, cancellationToken));
                    }
                }
            }

            for (int i = 0; i < searchDrawings.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) 
                {
                    break;
                }

                var drwFile = searchDrawings[i];

                var drw = m_App.Documents.PreCreate<IXDrawing>();
                drw.Path = drwFile;

                var drwDeps = drw.IterateDependencies(true).ToArray();

                var usedDocs = drwDeps.Intersect(docs,
                    new DocumentComparer()).ToArray();

                if (usedDocs.Any())
                {
                    foreach (var usedDoc in usedDocs)
                    {
                        List<IXDrawing> drwsList;

                        if (res.TryGetValue(usedDoc, out IXDrawing[] drws))
                        {
                            drwsList = new List<IXDrawing>(drws);
                        }
                        else
                        {
                            drwsList = new List<IXDrawing>();
                        }

                        drwsList.Add(drw);
                        res[usedDoc] = drwsList.ToArray();
                    }
                }

                prgHandler.Invoke((double)(i + 1) / (double)searchDrawings.Count);
            }

            return res;
        }

        private string[] GetDocumentsWorkingDirectories(IXDocument[] docs)
            => docs.Select(d => Path.GetDirectoryName(d.Path))
            .Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();

        private IEnumerable<string> TryGetAllFiles(string dir, string pattern, CancellationToken cancellationToken) 
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                string[] subDirs = null;
                string[] files = null;

                try
                {
                    subDirs = Directory.GetDirectories(dir, "*.*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                }

                try
                {
                    files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                }
                catch
                {
                }

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        yield return file;
                    }
                }

                if (subDirs != null)
                {
                    foreach (var subDir in subDirs)
                    {
                        foreach (var file in TryGetAllFiles(subDir, pattern, cancellationToken))
                        {
                            yield return file;
                        }
                    }
                }
            }
        }
    }
}
