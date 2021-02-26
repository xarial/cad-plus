using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Extensions.ViewModels
{
    public class DocumentVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public IXDocument Document { get; }

        private bool m_IsChecked;

        public bool IsChecked 
        {
            get => m_IsChecked;
            set 
            {
                m_IsChecked = value;
                this.NotifyChanged();
            }
        }

        public DocumentVM(IXDocument doc) 
        {
            Document = doc;
        }
    }

    public class ReferenceVM : DocumentVM
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DocumentVM[] Drawings 
        {
            get => m_Drawings;
            set 
            {
                m_Drawings = value;
                this.NotifyChanged();
            }
        }

        private DocumentVM[] m_Drawings;

        public ReferenceVM(IXDocument doc)  : base(doc)
        {
        }
    }
}
