//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.CadPlus.Batch.Extensions.ViewModels;

namespace Xarial.CadPlus.Batch.Extensions.UI
{
    public partial class ReferenceExtractorWindow : Window
    {
        private ReferenceExtractorVM m_RefExtractorVm;

        public ReferenceExtractorWindow()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        public static readonly DependencyProperty ReferencesGroupVisibleProperty =
            DependencyProperty.Register(
            nameof(ReferencesGroupVisible), typeof(bool),
            typeof(ReferenceExtractorWindow), new PropertyMetadata(true));

        public bool ReferencesGroupVisible
        {
            get { return (bool)GetValue(ReferencesGroupVisibleProperty); }
            set { SetValue(ReferencesGroupVisibleProperty, value); }
        }

        public void ResetColumnWidths(GridView gridView)
        {
            if (gridView != null)
            {
                foreach (var col in gridView.Columns)
                {
                    if (double.IsNaN(col.Width))
                    {
                        col.Width = col.ActualWidth; 
                    }
                    col.Width = double.NaN;
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_RefExtractorVm != null)
            {
                m_RefExtractorVm.GridDataChanged -= OnGridDataChanged;
            }

            if (e.NewValue is ReferenceExtractorVM)
            {
                m_RefExtractorVm = e.NewValue as ReferenceExtractorVM;
                m_RefExtractorVm.GridDataChanged += OnGridDataChanged;
            }
        }

        private void OnGridDataChanged()
            => ResetColumnWidths(grdRefs);

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
