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
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.XToolkit.Wpf.Controls;

namespace Xarial.CadPlus.Plus.Shared.UI
{
	public class BatchJobOperationCellContentSelector : ICellContentSelector
	{
		public object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell)
		{
			var jobItem = dataItem as BatchJobItemVM;
			var operationColumn = (BatchJobItemOperationDefinitionVM)column.Header;

			return jobItem.Operations.First(o => o.JobItemOperation.Definition == operationColumn.Definition);
		}
	}

	public partial class BatchJobReportControl : UserControl
    {
        public BatchJobReportControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DataGridStyleProperty =
            DependencyProperty.Register(
                nameof(DataGridStyle), typeof(Style),
                typeof(BatchJobReportControl), new PropertyMetadata(OnStyleChanged));

        private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BatchJobReportControl)d).SetStyle((Style)e.NewValue);
        }

        private void SetStyle(Style style) 
        {
            grdResults.Style = style;
        }

        public Style DataGridStyle
        {
            get { return (Style)GetValue(DataGridStyleProperty); }
            set { SetValue(DataGridStyleProperty, value); }
        }

		private void OnColumnsPreCreated(List<DataGridColumn> columns)
		{
			foreach (var operationCol in columns.Where(c => c.Header is BatchJobItemOperationDefinitionVM))
			{
				operationCol.SortMemberPath = nameof(BatchJobItemOperationVM.State) + "." + nameof(BatchJobItemStateBaseVM.Status);
			}
		}
	}
}
