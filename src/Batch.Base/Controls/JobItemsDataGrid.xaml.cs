//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
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
using Xarial.CadPlus.Batch.Base.Controls;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Controls;

namespace Xarial.CadPlus.Batch.Base.Controls
{
	public class MacroColumnHeaderVM 
	{
		public string Name { get; }
		public int MacroIndex { get; }

		public MacroColumnHeaderVM(string name, int index) 
		{
			Name = name;
			MacroIndex = index;
		}
	}

	public class MacroCellContentSelector : ICellContentSelector
	{
		public object SelectContent(object dataItem, DataGridColumn column, DataGridCell cell)
		{
			var jobItem = dataItem as JobItemDocumentVM;
			var macroColumn = (MacroColumnHeaderVM)column.Header;

			return jobItem.Macros[macroColumn.MacroIndex];
		}
	}

	public partial class JobItemsDataGrid : UserControl
    {
        public JobItemsDataGrid()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty DataGridStyleProperty =
			DependencyProperty.Register(
			nameof(DataGridStyle), typeof(Style),
			typeof(JobItemsDataGrid));

		public Style DataGridStyle
		{
			get { return (Style)GetValue(DataGridStyleProperty); }
			set { SetValue(DataGridStyleProperty, value); }
		}

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register(
			nameof(ItemsSource), typeof(IEnumerable),
			typeof(JobItemsDataGrid), new PropertyMetadata(OnItemsSourceChanged));

		public static readonly DependencyProperty CadDescriptorProperty =
			DependencyProperty.Register(
			nameof(CadDescriptor), typeof(ICadDescriptor),
			typeof(JobItemsDataGrid));

		public ICadDescriptor CadDescriptor
		{
			get { return (ICadDescriptor)GetValue(CadDescriptorProperty); }
			set { SetValue(CadDescriptorProperty, value); }
		}

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private static readonly DependencyPropertyKey MacroColumnsPropertyKey
			= DependencyProperty.RegisterReadOnly(nameof(MacroColumns),
				typeof(IEnumerable), typeof(JobItemsDataGrid),
				new FrameworkPropertyMetadata(default(IEnumerable),
					FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty MacroColumnsProperty
			= MacroColumnsPropertyKey.DependencyProperty;

		public IEnumerable MacroColumns
		{
			get { return (IEnumerable)GetValue(MacroColumnsProperty); }
			private set { SetValue(MacroColumnsPropertyKey, value); }
		}

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
		{
			var grd = d as JobItemsDataGrid;

			var jobItems = (e.NewValue as IEnumerable<JobItemDocumentVM>)?.ToArray();

			if (jobItems?.Any() == true)
			{
				var templateJob = jobItems.First();
				var columns = new MacroColumnHeaderVM[templateJob.Macros.Length];

				for (int i = 0; i < templateJob.Macros.Length; i++) 
				{
					columns[i] = new MacroColumnHeaderVM(templateJob.Macros[i].Name, i);
				}

				grd.MacroColumns = columns;
			}
			else
			{
				grd.MacroColumns = null;
			}
		}

		private void OnColumnsPreCreated(List<DataGridColumn> columns)
		{
			foreach (var macroCol in columns.Where(c => c.Header is MacroColumnHeaderVM)) 
			{
				macroCol.SortMemberPath = nameof(JobItemMacroVM.Status);
			}
		}
	}
}
