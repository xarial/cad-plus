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
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Common;
using System.Windows.Interop;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Shared.Styles;

namespace Xarial.CadPlus.Batch.Base
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            Application.Current.UsingMetroStyles();

            InitializeComponent();
        }
    }
}
