using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Xarial.CadPlus.Batch.StandAlone.Controls
{
    public partial class NewDocumentWindow
    {
        public NewDocumentWindow()
        {
            InitializeComponent();
        }

        private void OnJobSelected()
        {
            this.Close();
        }
    }
}
