using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Helpers;
using Xarial.XCad;

namespace Xarial.CadPlus.Batch.InApp.Services
{
    public class EscapeBatchExecutorCancelHandler : IDisposable
    {
        private readonly IBatchRunJobExecutor m_Exec;
        private readonly KeyboardHook m_Hook;
        private readonly IXApplication m_App;

        private readonly Dispatcher m_Dispatcher;

        private bool m_IsCancellingInProgress;

        public EscapeBatchExecutorCancelHandler(IBatchRunJobExecutor exec, IXApplication app, Dispatcher dispatcher) 
        {
            m_Exec = exec;
            m_App = app;
            m_Dispatcher = dispatcher;
            
            m_IsCancellingInProgress = false;

            m_Hook = new KeyboardHook(m_App.Process);
            m_Hook.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(KeyboardHook sender, System.Windows.Forms.Keys keys)
        {
            lock (m_Dispatcher)
            {
                if (keys == System.Windows.Forms.Keys.Escape && !m_IsCancellingInProgress)
                {
                    m_IsCancellingInProgress = true;

                    m_Dispatcher.Invoke(() =>
                    {
                        //NOTE: IMessageService with CAD specific message box may be supressed in a batch mode
                        if (System.Windows.MessageBox.Show("Do you want to cancel the current batch process?", "Batch+",
                            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            m_Exec.Cancel();
                        }
                    });

                    m_IsCancellingInProgress = false;
                }
            }
        }

        public void Dispose()
            => m_Hook.Dispose();
    }
}
