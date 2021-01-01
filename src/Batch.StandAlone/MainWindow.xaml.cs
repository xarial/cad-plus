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
using System.Windows.Interop;

namespace Xarial.CadPlus.XBatch.Base
{
    public partial class MainWindow
    {
        private readonly BatchManagerVM m_BatchManager;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += OnWindowClosing;

            XBatchApp app = null;

            try
            {
                app = (XBatchApp)Application.Current;

                m_BatchManager = app.Host.Services.GetService<BatchManagerVM>();

                this.DataContext = m_BatchManager;

                m_BatchManager.ParentWindowHandle = new WindowInteropHelper(this).EnsureHandle();
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

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !m_BatchManager.CanClose();
        }
    }
}
