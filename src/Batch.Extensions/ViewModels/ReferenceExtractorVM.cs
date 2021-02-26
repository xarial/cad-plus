using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Extensions.Models;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Extensions.ViewModels
{
    public enum ReferencesScope_e 
    {
        SourceDocumentsOnly,
        TopLevelReferences,
        AllReferences
    }

    public class ReferenceExtractorVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ReferenceVM[] m_References;

        public ReferenceVM[] References 
        {
            get => m_References;
            private set 
            {
                m_References = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<string> AdditionalDrawingFolders { get; }

        private ReferencesScope_e m_ReferencesScope;
        private bool m_FindDrawings;

        private bool m_IsInitializing;
        private double m_Progress;
        
        public ReferencesScope_e ReferencesScope
        {
            get => m_ReferencesScope;
            set
            {
                m_ReferencesScope = value;
                this.NotifyChanged();
            }
        }
        
        public bool FindDrawings
        {
            get => m_FindDrawings;
            set
            {
                m_FindDrawings = value;
                this.NotifyChanged();
            }
        }

        public bool IsInitializing
        {
            get => m_IsInitializing;
            private set
            {
                m_IsInitializing = value;
                this.NotifyChanged();
            }
        }

        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        private readonly IXDocument[] m_InputDocs;
        private readonly ReferenceExtractor m_RefsExtractor;

        public ReferenceExtractorVM(ReferenceExtractor refsExtractor, IXDocument[] docs) 
        {
            m_InputDocs = docs;
            m_RefsExtractor = refsExtractor;

            AdditionalDrawingFolders = new ObservableCollection<string>();
            AdditionalDrawingFolders.CollectionChanged += OnAdditionalDrawingFoldersChanged;
        }

        private void OnAdditionalDrawingFoldersChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            CollectDrawings();
        }

        public void CollectReferences()
        {
            var docs = m_RefsExtractor.GetAllReferences(m_InputDocs, ReferencesScope);

            References = docs.Select(d => new ReferenceVM(d)).ToArray();

            if (FindDrawings)
            {
                CollectDrawings();
            }
        }

        public void CollectDrawings() 
        {
            IsInitializing = true;

            var drawings = m_RefsExtractor.FindAllDrawings(References.Select(r => r.Document).ToArray(),
                AdditionalDrawingFolders.ToArray());

            var allDrws = new Dictionary<string, DocumentVM>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var reference in References) 
            {
                drawings.TryGetValue(reference.Document, out IXDrawing[] refDrws);

                var allDocVms = new List<DocumentVM>();

                if (refDrws != null)
                {
                    foreach (var refDrw in refDrws) 
                    {
                        if (!allDrws.TryGetValue(refDrw.Path, out DocumentVM refDrwVm)) 
                        {
                            refDrwVm = new DocumentVM(refDrw);
                            allDrws.Add(refDrw.Path, refDrwVm);
                        }
                    }
                }
                
                reference.Drawings = allDocVms.ToArray();
            }

            //TODO:associate drawings with view models
            IsInitializing = false;
        }
    }
}
