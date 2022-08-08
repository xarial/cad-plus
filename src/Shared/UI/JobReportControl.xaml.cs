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
	public class OperationCellContentSelector : ICellContentSelector
	{
		public object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell)
		{
			var jobItem = dataItem as JobItemVM;
			var operationColumn = (JobItemOperationDefinitionVM)column.Header;

			return jobItem.Operations.First(o => o.JobItemOperation.Definition == operationColumn.Definition);
		}
	}

	public partial class JobReportControl : UserControl
    {
        public JobReportControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DataGridStyleProperty =
            DependencyProperty.Register(
                nameof(DataGridStyle), typeof(Style),
                typeof(JobReportControl), new PropertyMetadata(OnStyleChanged));

        private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((JobReportControl)d).SetStyle((Style)e.NewValue);
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
			foreach (var operationCol in columns.Where(c => c.Header is JobItemOperationDefinitionVM))
			{
				operationCol.SortMemberPath = nameof(JobItemOperationVM.State);
			}
		}
	}
}
