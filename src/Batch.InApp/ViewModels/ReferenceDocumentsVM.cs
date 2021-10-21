//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Extensions;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.InApp.ViewModels
{
    public class ReferenceDocumentsVM : INotifyPropertyChanged
    {
        public ICadDescriptor Descriptor { get; }

        private DocumentVM[] m_References;

        public DocumentVM[] References
        {
            get => m_References;
            private set
            {
                m_References = value;
                this.NotifyChanged();
            }
        }

        private DocumentVM[] m_AllReferences;
        private DocumentVM[] m_TopLevelReferences;
        private bool m_TopLevelOnly;
        private IXDocument m_Doc;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool TopLevelOnly
        {
            get => m_TopLevelOnly;
            set
            {
                m_TopLevelOnly = value;
                this.NotifyChanged();
                UpdateReferences();
            }
        }

        public ICommand TogglePartFilterCommand { get; }
        public ICommand ToggleAssemblyFilterCommand { get; }

        private bool m_CurPartToggle;
        private bool m_CurAssemblyToggle;

        public ReferenceDocumentsVM(ICadDescriptor cadEntDesc)
        {
            Descriptor = cadEntDesc;

            TogglePartFilterCommand = new RelayCommand(TogglePartFilter);
            ToggleAssemblyFilterCommand = new RelayCommand(ToggleAssemblyFilter);

            m_CurPartToggle = true;
            m_CurAssemblyToggle = true;
        }

        public void SetDocument(IXDocument doc)
        {
            m_Doc = doc;
            m_AllReferences = null;
            m_TopLevelReferences = null;
        }

        public void UpdateReferences()
        {
            if (TopLevelOnly)
            {
                if (m_TopLevelReferences == null)
                {
                    m_TopLevelReferences = CreateDocumentVMs(m_Doc.IterateDependencies(true).ToArray());
                }

                References = m_TopLevelReferences;
            }
            else 
            {
                if (m_AllReferences == null)
                {
                    m_AllReferences = CreateDocumentVMs(m_Doc.IterateDependencies(false).ToArray());
                }

                References = m_AllReferences;
            }
        }

        private DocumentVM[] CreateDocumentVMs(IXDocument3D[] refs)
            => new IXDocument[] { m_Doc }.Union(refs).Select(d => new DocumentVM(d)).ToArray();
        
        private void TogglePartFilter() 
        {
            m_CurPartToggle = !m_CurPartToggle;

            foreach (var reference in References.Where(r => r.Document is IXPart))
            {
                reference.IsChecked = m_CurPartToggle;
            }
        }

        private void ToggleAssemblyFilter() 
        {
            m_CurAssemblyToggle = !m_CurAssemblyToggle;

            foreach (var reference in References.Where(r => r.Document is IXAssembly))
            {
                reference.IsChecked = m_CurAssemblyToggle;
            }
        }
    }
}
