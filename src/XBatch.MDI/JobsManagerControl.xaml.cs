using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public partial class JobsManagerControl : UserControl
    {
		private List<JobDocument> m_UserClosedDocsQueue;

        public JobsManagerControl()
        {
            InitializeComponent();
			ctrlDock.ActiveDocumentChanged += OnActiveDocumentChanged;
			m_UserClosedDocsQueue = new List<JobDocument>();
		}

		public static readonly DependencyProperty JobDocumentTemplateProperty =
			DependencyProperty.Register(
			nameof(JobDocumentTemplate), typeof(DataTemplate),
			typeof(JobsManagerControl));

		public DataTemplate JobDocumentTemplate
		{
			get { return (DataTemplate)GetValue(JobDocumentTemplateProperty); }
			set { SetValue(JobDocumentTemplateProperty, value); }
		}

		public static readonly DependencyProperty JobResultLogTemplateProperty =
			DependencyProperty.Register(
			nameof(JobResultLogTemplate), typeof(DataTemplate),
			typeof(JobsManagerControl));

		public string JobResultLogTemplate
		{
			get { return (string)GetValue(JobResultLogTemplateProperty); }
			set { SetValue(JobResultLogTemplateProperty, value); }
		}

		public static readonly DependencyProperty JobResultsTemplateProperty =
			DependencyProperty.Register(
			nameof(JobResultsTemplate), typeof(DataTemplate),
			typeof(JobsManagerControl));

		public DataTemplate JobResultsTemplate
		{
			get { return (DataTemplate)GetValue(JobResultsTemplateProperty); }
			set { SetValue(JobResultsTemplateProperty, value); }
		}

		public static readonly DependencyProperty JobSettingsTemplateProperty =
			DependencyProperty.Register(
			nameof(JobSettingsTemplate), typeof(DataTemplate),
			typeof(JobsManagerControl));

		public DataTemplate JobSettingsTemplate
		{
			get { return (DataTemplate)GetValue(JobSettingsTemplateProperty); }
			set { SetValue(JobSettingsTemplateProperty, value); }
		}

		public static readonly DependencyProperty JobResultSummaryProperty =
			DependencyProperty.Register(
			nameof(JobResultSummary), typeof(DataTemplate),
			typeof(JobsManagerControl));

		public DataTemplate JobResultSummary
		{
			get { return (DataTemplate)GetValue(JobResultSummaryProperty); }
			set { SetValue(JobResultSummaryProperty, value); }
		}

		public static readonly DependencyProperty ActiveJobDocumentProperty =
			DependencyProperty.Register(
			nameof(ActiveJobDocument), typeof(IJobDocument),
			typeof(JobsManagerControl), 
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnActiveJobDocumentChanged));

		public IJobDocument ActiveJobDocument
		{
			get { return (IJobDocument)GetValue(ActiveJobDocumentProperty); }
			set { SetValue(ActiveJobDocumentProperty, value); }
		}
		
		public static readonly DependencyProperty JobDocumentsSourceProperty =
			DependencyProperty.Register(
			nameof(JobDocumentsSource), typeof(IList),
			typeof(JobsManagerControl), new PropertyMetadata(OnJobDocumentsSourceChanged));

		public IList JobDocumentsSource
		{
			get { return (IList)GetValue(JobDocumentsSourceProperty); }
			set { SetValue(JobDocumentsSourceProperty, value); }
		}

		private static void OnActiveJobDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
		{
			var jobsCtrl = d as JobsManagerControl;
			jobsCtrl.SetActiveJobDocument(e.NewValue as IJobDocument);
		}

		private void SetActiveJobDocument(IJobDocument doc) 
		{
			if (doc != null) 
			{
				var docItem = ctrlDock.Documents.FirstOrDefault(d => (d as JobDocument).Document == doc);

				if (docItem == null) 
				{
					throw new NullReferenceException("Specified document is not present in documents list");
				}

				if (!docItem.IsActiveDocument)
				{
					docItem.Activate();
				}
			}
		}

		private static void OnJobDocumentsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
		{
			var jobsCtrl = d as JobsManagerControl;

			if (e.OldValue is INotifyCollectionChanged)
			{
				(e.OldValue as INotifyCollectionChanged).CollectionChanged -= jobsCtrl.OnJobsDocumentsSourceCollectionChanged;
			}

			if (e.NewValue is INotifyCollectionChanged) 
			{
				(e.NewValue as INotifyCollectionChanged).CollectionChanged += jobsCtrl.OnJobsDocumentsSourceCollectionChanged;
			}

			jobsCtrl.UpdateDocuments();
		}

		private void OnJobsDocumentsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateDocuments();
		}

		private void OnActiveDocumentChanged(object sender, EventArgs e)
		{
			ActiveJobDocument = (ctrlDock.ActiveDocument as JobDocument)?.Document;
		}

		private void UpdateDocuments() 
		{
			if (ctrlDock.IsLoaded)
			{
				var curDocs = new List<IJobDocument>();
				
				foreach (JobDocument docItem in ctrlDock.Documents)
				{
					var jobDoc = docItem.Document;

					if (JobDocumentsSource?.Contains(jobDoc) == true)
					{
						curDocs.Add(jobDoc);
					}
					else
					{
						if (!m_UserClosedDocsQueue.Contains(docItem))
						{
							docItem.Close();
						}
						else 
						{
							m_UserClosedDocsQueue.Remove(docItem);
						}
						
						docItem.Closing -= OnJobDocumentClosing;
					}
				}

				if (JobDocumentsSource != null)
				{
					foreach (IJobDocument newDoc in JobDocumentsSource)
					{
						if (!curDocs.Contains(newDoc))
						{
							var jobDoc = new JobDocument(newDoc);
							jobDoc.Closing += OnJobDocumentClosing;
							jobDoc.Show(ctrlDock);
						}
					}
				}
			}
			else 
			{
				ctrlDock.Loaded += OnControlLoaded;
			}
		}
		
		private void OnJobDocumentClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var jobDoc = sender as JobDocument;
			m_UserClosedDocsQueue.Add(jobDoc);
			JobDocumentsSource.Remove(jobDoc.Document);
		}

		private void OnControlLoaded(object sender, RoutedEventArgs e)
		{
			ctrlDock.Loaded -= OnControlLoaded;
			UpdateDocuments();
		}
	}
}
