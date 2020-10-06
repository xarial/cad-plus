//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
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

namespace Xarial.CadPlus.Common.Controls
{
    public class TaskDashboard : Control
    {
        private TextBox m_LogTextBox;

        static TaskDashboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TaskDashboard), 
                new FrameworkPropertyMetadata(typeof(TaskDashboard)));
        }

        public override void OnApplyTemplate()
        {
            m_LogTextBox = (TextBox)this.Template.FindName("PART_LogTextBox", this);

            SetLogText(LogSource);
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
            nameof(Progress), typeof(double),
            typeof(TaskDashboard));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty LogSourceProperty =
            DependencyProperty.Register(
            nameof(LogSource), typeof(IEnumerable),
            typeof(TaskDashboard), new PropertyMetadata(OnLogSourceChanged));

        public IEnumerable LogSource
        {
            get { return (IEnumerable)GetValue(LogSourceProperty); }
            set { SetValue(LogSourceProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
            nameof(CancelCommand), typeof(ICommand),
            typeof(TaskDashboard));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        private void LoadLog(IEnumerable newLog, IEnumerable oldLog) 
        {
            if (oldLog is INotifyCollectionChanged) 
            {
                (oldLog as INotifyCollectionChanged).CollectionChanged -= OnLogCollectionChanged;
            }

            if (newLog != null)
            {
                SetLogText(newLog);

                if (newLog is INotifyCollectionChanged)
                {
                    (newLog as INotifyCollectionChanged).CollectionChanged += OnLogCollectionChanged;
                }
            }
            else 
            {
                m_LogTextBox.Text = "";
            }
        }

        private void SetLogText(IEnumerable lines)
        {
            if (m_LogTextBox != null && lines != null)
            {
                var logText = new StringBuilder();

                foreach (var line in lines)
                {
                    logText.AppendLine(line?.ToString());
                }

                m_LogTextBox.Dispatcher.Invoke(() => m_LogTextBox.Text = logText.ToString());
            }
        }

        private void OnLogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetLogText(sender as IEnumerable);
        }

        private static void OnLogSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var dashboard = (TaskDashboard)d;
            dashboard.LoadLog(e.NewValue as IEnumerable, e.OldValue as IEnumerable);
        }
    }
}
