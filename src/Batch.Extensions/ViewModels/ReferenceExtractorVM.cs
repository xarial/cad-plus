//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Xarial.CadPlus.Batch.Extensions.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Extensions.ViewModels
{
    public enum ReferencesScope_e 
    {
        [Title("Source Documents")]
        SourceDocumentsOnly,

        [Title("Top Level Dependencies")]
        TopLevelDependencies,

        [Title("All Dependencies")]
        AllDependencies
    }

    public class ReferenceExtractorVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event Action GridDataChanged;

        public IReadOnlyCollection<ReferenceVM> References => m_References;

        private ObservableCollection<ReferenceVM> m_References;

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

        private bool? m_AllDocumentsIsChecked;

        public bool? AllDocumentsIsChecked 
        {
            get => m_AllDocumentsIsChecked;
            set 
            {
                m_AllDocumentsIsChecked = value;
                this.NotifyChanged();
                BatchSetCheck(References, m_AllDocumentsIsChecked);
            }
        }

        private bool? m_AllDrawingsIsChecked;

        public bool? AllDrawingsIsChecked
        {
            get => m_AllDrawingsIsChecked;
            set
            {
                m_AllDrawingsIsChecked = value;
                this.NotifyChanged();
                BatchSetCheck(m_DrawingVms, m_AllDrawingsIsChecked);
            }
        }

        public ICadDescriptor Descriptor { get; }

        private readonly IXDocument[] m_InputDocs;
        private readonly ReferenceExtractor m_RefsExtractor;

        private readonly IXLogger m_Logger;
        private readonly IMessageService m_MsgSvc;

        private int m_CheckedReferencesCount;
        private int m_CheckedDrawingsCount;

        private object m_Lock = new object();

        private readonly CancellationToken m_CancellationToken;

        public ReferenceExtractorVM(ReferenceExtractor refsExtractor,
            IXDocument[] docs, ICadDescriptor cadEntDesc, IXLogger logger, IMessageService msgSvc,
            ReferencesScope_e scope, bool findDrws, CancellationToken cancellationToken) 
        {
            m_InputDocs = docs;
            m_RefsExtractor = refsExtractor;

            m_CancellationToken = cancellationToken;

            m_Logger = logger;
            m_MsgSvc = msgSvc;

            m_ReferencesScope = scope;
            m_FindDrawings = findDrws;

            Descriptor = cadEntDesc;

            m_DrawingVms = new List<DocumentVM>();

            m_References = new ObservableCollection<ReferenceVM>();
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
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private async void OnScopeChanged()
        {
            try
            {
                await CollectReferencesAsync();
            }
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private async void OnFindDrawingChanged()
        {
            try
            {
                await CollectDrawingsAsync();
            }
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        public async Task CollectReferencesAsync()
        {
            IsInitializing = true;
            Progress = null;

            try
            {
                var docs = await Task.Run(() => m_RefsExtractor.GetAllReferences(m_InputDocs, ReferencesScope));

                m_References.Clear();
                m_CheckedReferencesCount = 0;
                
                foreach (var doc in docs)
                {
                    if (m_CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var refVm = new ReferenceVM(doc);
                    refVm.CheckedChanged += OnReferenceCheckedChanged;

                    if (refVm.IsChecked)
                    {
                        m_CheckedReferencesCount++;
                    }

                    m_References.Add(refVm);
                }

                ResolveAllDocumensIsCheckedFlag();

                await CollectDrawingsAsync();
            }
            finally 
            {
                IsInitializing = false;
            }
        }

        private readonly List<DocumentVM> m_DrawingVms;

        public async Task CollectDrawingsAsync()
        {
            IsInitializing = true;
            Progress = null;
            m_CheckedDrawingsCount = 0;

            try
            {
                m_DrawingVms.Clear();

                Dictionary<IXDocument, IXDrawing[]> drawings;

                if (FindDrawings)
                {
                    var allRefs = References.Select(r => r.Document).ToArray();

                    drawings = await Task.Run(() => m_RefsExtractor.FindAllDrawings(allRefs,
                        AdditionalDrawingFolders.ToArray(), p => Progress = p, m_CancellationToken));
                }
                else 
                {
                    drawings = new Dictionary<IXDocument, IXDrawing[]>();
                }

                var allDocVms = References.ToDictionary(x => x.Document.Path,
                        x => (DocumentVM)x, StringComparer.CurrentCultureIgnoreCase);

                foreach (var reference in References)
                {
                    if (m_CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

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

                            if (!m_DrawingVms.Contains(refDrwVm))
                            {
                                if (refDrwVm.IsChecked)
                                {
                                    m_CheckedDrawingsCount++;
                                }

                                refDrwVm.CheckedChanged += OnReferenceDrawingCheckedChanged;

                                m_DrawingVms.Add(refDrwVm);
                            }

                            reference.Drawings.Add(refDrwVm);
                        }
                    }
                }

                ResolveAllDrawingsIsCheckedFlag();
                GridDataChanged?.Invoke();
            }
            finally 
            {
                IsInitializing = false;
            }
        }

        private void OnReferenceDrawingCheckedChanged(DocumentVM drw, bool isChecked)
        {
            m_CheckedDrawingsCount += isChecked ? 1 : -1;
            ResolveAllDrawingsIsCheckedFlag();
        }

        private void OnReferenceCheckedChanged(DocumentVM refDoc, bool isChecked)
        {
            m_CheckedReferencesCount += isChecked ? 1 : -1;
            ResolveAllDocumensIsCheckedFlag();
        }

        private void BatchSetCheck(IEnumerable<DocumentVM> coll, bool? isChecked)
        {
            if (isChecked.HasValue)
            {
                foreach (var refDoc in coll)
                {
                    refDoc.IsChecked = isChecked.Value;
                }
            }
        }

        private void ResolveAllDocumensIsCheckedFlag()
        {
            m_AllDocumentsIsChecked = GetBatchCheckedFlag(m_CheckedReferencesCount, References.Count);
            this.NotifyChanged(nameof(AllDocumentsIsChecked));
        }

        private void ResolveAllDrawingsIsCheckedFlag()
        {
            m_AllDrawingsIsChecked = GetBatchCheckedFlag(m_CheckedDrawingsCount, m_DrawingVms.Count);
            this.NotifyChanged(nameof(AllDrawingsIsChecked));
        }

        private bool? GetBatchCheckedFlag(int checkedCount, int totalCount) 
        {
            if (checkedCount == 0)
            {
                return false;
            }
            else if (checkedCount == totalCount)
            {
                return true;
            }
            else
            {
                return null;
            }
        }
    }

    public static class ReferenceExtractorVMExtension 
    {
        public static IEnumerable<IXDocument> GetCheckedDocuments(this ReferenceExtractorVM vm) 
        {
            return vm.References
                .Union(vm.References.SelectMany(d => d.Drawings))
                .Distinct()
                .Where(d => d.IsChecked)
                .Select(d => d.Document);
        }
    }
}
