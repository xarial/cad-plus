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
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.XBatch.Base
{
    public partial class MainWindow
    {
        private BatchManagerVM m_BatchManager;

        private readonly XBatchApp m_App;

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += OnWindowClosing;
            m_App = (XBatchApp)Application.Current;
            m_App.Host.Started += OnHostStarted;
        }

        private void OnHostStarted()
        {
            try
            {
                m_BatchManager = m_App.Host.Services.GetService<BatchManagerVM>();

                this.DataContext = m_BatchManager;

                m_BatchManager.ParentWindowHandle = new WindowInteropHelper(this).EnsureHandle();
            }
            catch (Exception ex)
            {
                IMessageService msgSvc;

                try
                {
                    msgSvc = m_App.Host.Services.GetService<IMessageService>();
                }
                catch
                {
                    msgSvc = new GenericMessageService("Batch+");
                }

                msgSvc.ShowError(ex.ParseUserError(out _));
                Environment.Exit(1);
            }
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !m_BatchManager.CanClose();
        }
    }
}
