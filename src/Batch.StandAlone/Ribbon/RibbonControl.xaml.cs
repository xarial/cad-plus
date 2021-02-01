using Fluent;
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
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Plus.Shared.UI
{
    public partial class RibbonControl : UserControl
    {
		private const string COMMAND_TEMPLATE_SELECTOR_RES_NAME = "ribbonGrpCmdTemplateSelector";

		public RibbonControl()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty CommandManagerProperty =
			DependencyProperty.Register(
			nameof(CommandManager), typeof(IRibbonCommandManager),
			typeof(RibbonControl), new PropertyMetadata(OnCommandManagerChanged));

		public IRibbonCommandManager CommandManager
		{
			get { return (IRibbonCommandManager)GetValue(CommandManagerProperty); }
			set { SetValue(CommandManagerProperty, value); }
		}

		private static void OnCommandManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
		{
			(d as RibbonControl).LoadCommandManager((IRibbonCommandManager)e.NewValue);
		}

		private void LoadCommandManager(IRibbonCommandManager cmdMgr)
		{
			ctrlRibbon.Tabs.Clear();

			if (cmdMgr?.Tabs != null) 
			{
				foreach (var tab in cmdMgr.Tabs) 
				{
					var tabItem = new RibbonTabItem()
					{
						Header = tab.Title,
						DataContext = tab
					};

					ctrlRibbon.Tabs.Add(tabItem);

					if (tab.Groups != null)
					{
						foreach (var group in tab.Groups)
						{
							var groupItem = new RibbonGroupBox()
							{
								Header = group.Title,
								DataContext = group,
								ItemsSource = group.Commands,
								ItemTemplateSelector = (DataTemplateSelector)this.FindResource(COMMAND_TEMPLATE_SELECTOR_RES_NAME)
							};

							tabItem.Groups.Add(groupItem);
						}
					}
				}
			}

			ctrlRibbon.SelectedTabIndex = 0;
		}
	}
}
