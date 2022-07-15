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

        private bool m_IsCancellingInProgress;

        public BatchOperationCancellationService(CancellationTokenSource cancellationTokenSource, Process prc)
        {
            m_CancellationTokenSource = cancellationTokenSource;

            m_IsCancellingInProgress = false;

            m_Hook = new KeyboardHook(prc);
            m_Hook.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(KeyboardHook sender, System.Windows.Forms.Keys keys)
        {
            if (keys == System.Windows.Forms.Keys.Escape && !m_IsCancellingInProgress)
            {
                m_IsCancellingInProgress = true;

                Cancel();

                m_IsCancellingInProgress = false;
            }
        }

        protected virtual void Cancel() => m_CancellationTokenSource.Cancel();

        public void Dispose() => m_Hook.Dispose();
    }

    public class BatchOperationInteractiveCancellationService : BatchOperationCancellationService
    {
        private readonly Dispatcher m_Dispatcher;

        private readonly string m_ConfirmationMsg;
        private readonly string m_ConfirmationMsgTitle;

        public BatchOperationInteractiveCancellationService(CancellationTokenSource cancellationTokenSource, Process prc, Dispatcher dispatcher,
            string confirmationMsg, string confirmationMsgTitle) : base(cancellationTokenSource, prc)
        {
            m_Dispatcher = dispatcher;

            m_ConfirmationMsg = confirmationMsg;
            m_ConfirmationMsgTitle = confirmationMsgTitle;
        }

        protected override void Cancel()
        {
            lock (m_Dispatcher)
            {
                m_Dispatcher.Invoke(() =>
                {
                    //NOTE: IMessageService with CAD specific message box may be supressed in a batch mode
                    if (System.Windows.MessageBox.Show(m_ConfirmationMsg, m_ConfirmationMsgTitle,
                        System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                    {
                        base.Cancel();
                    }
                }, DispatcherPriority.Send);
            }
        }
    }
}
