using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xarial.CadPlus.XBatch.MDI
{
    partial class JobDocument : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IJobDocument Document { get; }

        public JobDocument()
        {
            InitializeComponent();
        }

        public JobDocument(IJobDocument document) 
            : this()
        {
            Document = document;
            
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Document)));
        }
    }
}
