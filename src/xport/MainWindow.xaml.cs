//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Shared.Styles;

namespace Xarial.CadPlus.Xport
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Application.Current.UsingMetroStyles();

            InitializeComponent();
        }
    }
}