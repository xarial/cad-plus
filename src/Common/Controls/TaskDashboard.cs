//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
        static TaskDashboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TaskDashboard), 
                new FrameworkPropertyMetadata(typeof(TaskDashboard)));
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
            typeof(TaskDashboard));

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
    }
}
