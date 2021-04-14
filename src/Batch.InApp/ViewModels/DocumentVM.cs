using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.InApp.ViewModels
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
            m_IsChecked = true;
        }
    }
}
