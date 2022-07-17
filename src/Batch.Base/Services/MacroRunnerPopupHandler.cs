using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IMacroRunnerPopupHandler : IDisposable
    {
        event Action<IMacroRunnerPopupHandler, Exception> MacroUserError;
        void Start(IXApplication app);
        void Stop();
    }

    public class MacroRunnerPopupHandler : IMacroRunnerPopupHandler
    {
        private const double POPUP_KILLER_PING_SECS = 2;

        public event Action<IMacroRunnerPopupHandler, Exception> MacroUserError;

        private readonly bool m_SilentMode;
        private readonly IPopupKiller m_PopupKiller;
        private readonly IXLogger m_Logger;

        public MacroRunnerPopupHandler(IPopupKiller popupKiller, IXLogger logger, bool silentMode) 
        {
            m_PopupKiller = popupKiller;
            m_PopupKiller.ShouldClosePopup += OnShouldClosePopup;
            m_PopupKiller.PopupNotClosed += OnPopupNotClosed;

            m_Logger = logger;

            m_SilentMode = silentMode;
        }

        private void OnPopupNotClosed(Process prc, IntPtr hWnd)
        {
            //VBA error popup cannot be closed automatically
            TryHandleVbaExceptionPopup(hWnd);
        }

        private void OnShouldClosePopup(Process prc, IntPtr hWnd, ref bool close)
        {
            if (m_SilentMode)
            {
                //attempt to close all popups in the silent mode
                close = true;
            }
            else
            {
                close = false;
                //only close VBA error popup
                TryHandleVbaExceptionPopup(hWnd);
            }
        }

        private void TryHandleVbaExceptionPopup(IntPtr hWnd)
        {
            using (var vbaErrPopup = new VbaErrorPopup(hWnd))
            {
                if (vbaErrPopup.IsVbaErrorPopup)
                {
                    m_Logger.Log($"Closing VBA Error popup window: {hWnd}", LoggerMessageSeverity_e.Debug);

                    MacroUserError?.Invoke(this, new VbaMacroException(vbaErrPopup.ErrorText));

                    vbaErrPopup.Close();
                }
                else
                {
                    m_Logger.Log($"Blocking popup window is not closed: {hWnd}", LoggerMessageSeverity_e.Debug);

                    //Log?.Invoke("Failed to close the blocking popup window");
                }
            }
        }

        public void Start(IXApplication app)
            => m_PopupKiller.Start(app.Process, TimeSpan.FromSeconds(POPUP_KILLER_PING_SECS));

        public void Stop()
        {
            try
            {
                if (m_PopupKiller != null && m_PopupKiller.IsStarted)
                {
                    m_Logger.Log($"Trying to kill the popup killer", LoggerMessageSeverity_e.Debug);

                    m_PopupKiller.Stop();
                }
            }
            catch
            {
            }
        }

        public void Dispose() => m_PopupKiller.Dispose();
    }

    public interface IMacroRunnerPopupHandlerFactory 
    {
        IMacroRunnerPopupHandler Create(bool silent);
    }

    public class MacroRunnerPopupHandlerFactory : IMacroRunnerPopupHandlerFactory
    {
        private readonly IPopupKillerFactory m_PopupKillerFact;
        private readonly IXLogger m_Logger;

        public MacroRunnerPopupHandlerFactory(IPopupKillerFactory popupKillerFact, IXLogger logger) 
        {
            m_PopupKillerFact = popupKillerFact;
            m_Logger = logger;
        }

        public IMacroRunnerPopupHandler Create(bool silentMode)
            => new MacroRunnerPopupHandler(m_PopupKillerFact.Create(), m_Logger, silentMode);
    }
}
