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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Controls
{
    public class JobItemStateControl : Control
    {
        static JobItemStateControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(JobItemStateControl), new FrameworkPropertyMetadata(typeof(JobItemStateControl)));
        }

        private Hyperlink m_ShowIssuesLink;
        private Popup m_IssuesPopup;

        public override void OnApplyTemplate()
        {
            m_ShowIssuesLink = (Hyperlink)this.Template.FindName("PART_ShowIssuesLink", this);
            m_IssuesPopup = (Popup)this.Template.FindName("PART_IssuesPopup", this);

            m_ShowIssuesLink.Click += OnShowIssuesLinkClick;
        }

        private void OnShowIssuesLinkClick(object sender, RoutedEventArgs e)
        {
            m_IssuesPopup.IsOpen = true;
        }

        public static readonly DependencyProperty IssuesProperty =
            DependencyProperty.Register(
            nameof(Issues), typeof(IEnumerable<IBatchJobItemIssue>),
            typeof(JobItemStateControl));

        public IEnumerable<IBatchJobItemIssue> Issues
        {
            get { return (IEnumerable<IBatchJobItemIssue>)GetValue(IssuesProperty); }
            set { SetValue(IssuesProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
            nameof(Status), typeof(BatchJobItemStateStatus_e),
            typeof(JobItemStateControl));

        public BatchJobItemStateStatus_e Status
        {
            get { return (BatchJobItemStateStatus_e)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusIconVisibilityProperty =
            DependencyProperty.Register(
            nameof(StatusIconVisibility), typeof(Visibility),
            typeof(JobItemStateControl));

        public Visibility StatusIconVisibility
        {
            get { return (Visibility)GetValue(StatusIconVisibilityProperty); }
            set { SetValue(StatusIconVisibilityProperty, value); }
        }
    }
}
