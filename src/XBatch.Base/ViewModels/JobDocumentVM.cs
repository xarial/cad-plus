using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobDocumentVM : INotifyPropertyChanged, IJobDocument
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_Name;

        public string Name
        {
            get => m_Name;
            private set 
            {
                m_Name = value;
                this.NotifyChanged();
            }
        }

        public IJobSettings Settings => null;

        public IEnumerable<IJobResult> Results => null;

        public JobDocumentVM(string name) 
        {
            Name = name;
        }
    }
}
