//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Applications;

namespace Xarial.CadPlus.Batch.StandAlone.Controls
{
    public partial class JobSelectorControl : UserControl
    {
		public event Action Selected;

        public JobSelectorControl()
        {
            InitializeComponent();
        }

		public static readonly DependencyProperty AppProvidersProperty =
			DependencyProperty.Register(
			nameof(AppProviders), typeof(ICadApplicationInstanceProvider[]),
			typeof(JobSelectorControl));

		public ICadApplicationInstanceProvider[] AppProviders
		{
			get { return (ICadApplicationInstanceProvider[])GetValue(AppProvidersProperty); }
			set { SetValue(AppProvidersProperty, value); }
		}

		public static readonly DependencyProperty CreateDocumentCommandProperty =
			DependencyProperty.Register(
			nameof(CreateDocumentCommand), typeof(ICommand),
			typeof(JobSelectorControl));

		public ICommand CreateDocumentCommand
		{
			get { return (ICommand)GetValue(CreateDocumentCommandProperty); }
			set { SetValue(CreateDocumentCommandProperty, value); }
		}

		private void OnCreateDocumentClick(object sender, RoutedEventArgs e)
		{
			var appProvider = (sender as Button).DataContext as ICadApplicationInstanceProvider;
			CreateDocumentCommand?.Execute(appProvider.Descriptor.ApplicationId);
			Selected?.Invoke();
		}
	}
}
