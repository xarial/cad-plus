//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Interop;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Xport.Models;
using Xarial.CadPlus.Xport.ViewModels;

namespace Xarial.CadPlus.Xport
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new ExporterVM(
                new ExporterModel(),
                new GenericMessageService("eXport+"));

            vm.ParentWindowHandle = new WindowInteropHelper(this).EnsureHandle();

            this.DataContext = vm;
        }
    }
}