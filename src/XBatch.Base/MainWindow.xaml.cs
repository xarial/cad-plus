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
using Xarial.CadPlus.XBatch.Base.Services;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.XToolkit.Reporting;
using Xarial.CadPlus.Common;

namespace Xarial.CadPlus.XBatch.Base
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            XBatchApp app = null;

            try
            {
                app = (XBatchApp)Application.Current;

                var vm = app.Host.Services.GetService<BatchManagerVM>();
                                
                this.DataContext = vm;
            }
            catch (Exception ex)
            {
                IMessageService msgSvc;

                try
                {
                    msgSvc = app.Host.Services.GetService<IMessageService>();
                }
                catch 
                {
                    msgSvc = new GenericMessageService("Batch+");
                }

                msgSvc.ShowError(ex.ParseUserError(out _));
            }
        }
    }
}
