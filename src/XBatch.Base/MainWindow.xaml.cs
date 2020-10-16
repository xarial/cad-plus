//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var msgService = new MessageService("xBatch");

            try
            {
                var appProvider = (Application.Current as XBatchApp).GetApplicationProvider();
                var batchRunnerModel = new Models.BatchRunnerModel(appProvider);

                var vm = new JobsManagerVM(batchRunnerModel, msgService);
                this.DataContext = vm;
            }
            catch (Exception ex)
            {
                msgService.ShowError(ex.ParseUserError(out _));
            }
        }
    }
}
