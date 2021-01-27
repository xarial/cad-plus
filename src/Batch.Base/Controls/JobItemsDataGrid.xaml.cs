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
using Xarial.CadPlus.XBatch.Base.Converters;
using Xarial.CadPlus.XBatch.Base.ViewModels;

namespace Xarial.CadPlus.XBatch.Base.Controls
{
    public partial class JobItemsDataGrid : UserControl
    {
        public JobItemsDataGrid()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register(
			nameof(ItemsSource), typeof(IList),
			typeof(JobItemsDataGrid), new PropertyMetadata(OnItemsSourceChanged));

		public string ItemsSource
		{
			get { return (string)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
		{
			var grd = d as JobItemsDataGrid;
			grd.LoadItems(e.NewValue as IEnumerable<JobItemFileVM>);
		}

		//TODO: replace with XDataGrid with dynamic columns

		private DataGridColumn CreateStatusColumn(string header, Binding binding) 
		{	
			var elemFact = new FrameworkElementFactory(typeof(Image));
			
			elemFact.SetValue(Image.SourceProperty, binding);
			elemFact.SetValue(Image.WidthProperty, 16d);
			elemFact.SetValue(Image.HeightProperty, 16d);

			var cellTemplate = new DataTemplate()
			{
				VisualTree = elemFact
			};

			var statusCol = new DataGridTemplateColumn()
			{
				IsReadOnly = true,
				Header = header,
				CellTemplate = cellTemplate,
				Width = DataGridLength.SizeToHeader,
				SortMemberPath = binding.Path.Path
			};

			return statusCol;
		}

		private void LoadItems(IEnumerable<JobItemFileVM> items) 
		{
			dgrdJobItems.Columns.Clear();

			if (items?.Any() == true)
			{
				var fileStatusCol = CreateStatusColumn("Status", new Binding("Status")
				{
					Converter = new JobItemStatusToImageConverter(),
					Mode = BindingMode.OneWay
				});

				dgrdJobItems.Columns.Add(fileStatusCol);

				var fileCol = new DataGridTextColumn()
				{
					Header = "File",
					Binding = new Binding("Name")
				};

				dgrdJobItems.Columns.Add(fileCol);

				var headerMacros = items.First().Macros;

				for (int i = 0; i < headerMacros.Length; i++)
				{
					var macro = headerMacros[i];

					var macroCol = CreateStatusColumn(
						macro.Name,
						new Binding(string.Format("Macros[{0}].Status", i)) 
						{
							Converter = new JobItemStatusToImageConverter(),
							Mode = BindingMode.OneWay
						});

					dgrdJobItems.Columns.Add(macroCol);
				}
			}

			dgrdJobItems.ItemsSource = items;
		}
	}
}
