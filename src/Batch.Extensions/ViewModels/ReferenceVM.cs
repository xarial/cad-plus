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
    public class ReferenceVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IXDocument Document { get; }

        public IXDrawing[] Drawings 
        {
            get => m_Drawings;
            set 
            {
                m_Drawings = value;
                this.NotifyChanged();
            }
        }

        private IXDrawing[] m_Drawings;

        public ReferenceVM(IXDocument doc, IXDrawing[] drawings) 
        {
            Document = doc;
            Drawings = drawings;
        }
    }
}
