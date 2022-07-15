using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xarial.CadPlus.Plus.Shared.Helpers;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class BatchOperationCancellationService : IDisposable
    {
        private readonly CancellationTokenSource m_CancellationTokenSource;
        private readonly KeyboardHook m_Hook;

        private readonly Dispatcher m_Dispatcher;

        private bool m_IsCancellingInProgress;

        private readonly string m_ConfirmationMsg;
        private readonly string m_ConfirmationMsgTitle;

        public BatchOperationCancellationService(CancellationTokenSource cancellationTokenSource, Process prc, Dispatcher dispatcher,
            string confirmationMsg, string confirmationMsgTitle)
        {
            m_CancellationTokenSource = cancellationTokenSource;
            m_Dispatcher = dispatcher;

            m_ConfirmationMsg = confirmationMsg;
            m_ConfirmationMsgTitle = confirmationMsgTitle;

            m_IsCancellingInProgress = false;

            m_Hook = new KeyboardHook(prc);
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
                        if (System.Windows.MessageBox.Show(m_ConfirmationMsg, m_ConfirmationMsgTitle,
                            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            m_CancellationTokenSource.Cancel();
                        }
                    });

                    m_IsCancellingInProgress = false;
                }
            }
        }

        public void Dispose() => m_Hook.Dispose();
    }
}
