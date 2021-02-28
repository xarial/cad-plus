using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Batch.Extensions.Models;
using Xarial.CadPlus.Plus.Services;
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

        public ObservableCollection<ReferenceVM> References { get; }

        public ObservableCollection<string> AdditionalDrawingFolders { get; }

        private ReferencesScope_e m_ReferencesScope;
        private bool m_FindDrawings;

        private bool m_IsInitializing;
        private double? m_Progress;
        
        public ReferencesScope_e ReferencesScope
        {
            get => m_ReferencesScope;
            set
            {
                m_ReferencesScope = value;
                this.NotifyChanged();
                OnScopeChanged();
            }
        }
        
        public bool FindDrawings
        {
            get => m_FindDrawings;
            set
            {
                m_FindDrawings = value;
                this.NotifyChanged();
                OnFindDrawingChanged();
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

        public double? Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public ICadEntityDescriptor EntityDescriptor { get; }

        private readonly IXDocument[] m_InputDocs;
        private readonly ReferenceExtractor m_RefsExtractor;

        private object m_Lock = new object();

        public ReferenceExtractorVM(ReferenceExtractor refsExtractor,
            IXDocument[] docs, ICadEntityDescriptor cadEntDesc,
            ReferencesScope_e scope, bool findDrws) 
        {
            m_InputDocs = docs;
            m_RefsExtractor = refsExtractor;

            m_ReferencesScope = scope;
            m_FindDrawings = findDrws;

            EntityDescriptor = cadEntDesc;

            References = new ObservableCollection<ReferenceVM>();
            BindingOperations.EnableCollectionSynchronization(References, m_Lock);

            AdditionalDrawingFolders = new ObservableCollection<string>();
            AdditionalDrawingFolders.CollectionChanged += OnAdditionalDrawingFoldersChanged;
        }

        private async void OnAdditionalDrawingFoldersChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            try
            {
                await CollectDrawingsAsync();
            }
            catch
            {
                //TODO: show error
            }
        }

        private async void OnScopeChanged()
        {
            try
            {
                await CollectReferencesAsync();
            }
            catch
            {
                //TODO: show error
            }
        }

        private async void OnFindDrawingChanged()
        {
            try
            {
                await CollectDrawingsAsync();
            }
            catch
            {
                //TODO: show error
            }
        }

        public async Task CollectReferencesAsync()
        {
            IsInitializing = true;
            Progress = null;

            try
            {
                var docs = await Task.Run(() => m_RefsExtractor.GetAllReferences(m_InputDocs, ReferencesScope));

                References.Clear();

                foreach (var doc in docs)
                {
                    References.Add(new ReferenceVM(doc));
                }

                await CollectDrawingsAsync();
            }
            finally 
            {
                IsInitializing = false;
            }
        }

        public async Task CollectDrawingsAsync()
        {
            IsInitializing = true;
            Progress = null;

            try
            {
                Dictionary<IXDocument, IXDrawing[]> drawings;

                if (FindDrawings)
                {
                    var allRefs = References.Select(r => r.Document).ToArray();

                    drawings = await Task.Run(() => m_RefsExtractor.FindAllDrawings(allRefs,
                        AdditionalDrawingFolders.ToArray(), p => Progress = p));
                }
                else 
                {
                    drawings = new Dictionary<IXDocument, IXDrawing[]>();
                }

                var allDocVms = References.ToDictionary(x => x.Document.Path,
                        x => (DocumentVM)x, StringComparer.CurrentCultureIgnoreCase);

                foreach (var reference in References)
                {
                    reference.Drawings.Clear();

                    drawings.TryGetValue(reference.Document, out IXDrawing[] refDrws);

                    if (refDrws?.Any() == true)
                    {
                        foreach (var refDrw in refDrws)
                        {
                            if (!allDocVms.TryGetValue(refDrw.Path, out DocumentVM refDrwVm))
                            {
                                refDrwVm = new DocumentVM(refDrw);
                                allDocVms.Add(refDrw.Path, refDrwVm);
                            }

                            reference.Drawings.Add(refDrwVm);
                        }
                    }
                }
            }
            finally 
            {
                IsInitializing = false;
            }
        }
    }
}
