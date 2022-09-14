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
using Xarial.XCad.UI.PropertyPage;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Drawing.QrCode.UI
{
    public partial class QrCodeExpressionBoxControl : UserControl, IXCustomControl, INotifyPropertyChanged
    {
        public event CustomControlValueChangedDelegate ValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private object m_Value;

        public QrCodeExpressionBoxControl()
        {
            InitializeComponent();
        }

        public object Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                ValueChanged?.Invoke(this, value);
                this.NotifyChanged();
            }
        }
    }
}
