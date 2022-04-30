﻿//*********************************************************************
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

namespace Xarial.CadPlus.Batch.Base.Controls
{
    public partial class JobItemStatusControl : UserControl
    {
        public JobItemStatusControl()
        {
            InitializeComponent();
        }

        private void OnShowErrorClick(object sender, RoutedEventArgs e)
        {
            popupError.IsOpen = true;
        }
    }
}
