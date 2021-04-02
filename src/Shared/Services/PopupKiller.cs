//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public interface IPopupKiller : IDisposable
    {
        event Action<Process> PopupNotClosed;

        bool IsStarted { get; }
        void Start(Process prc, TimeSpan period, string popupClassName = "#32770");
        void Stop();
    }

    public class PopupKiller : IPopupKiller
    {
        private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        public event Action<Process> PopupNotClosed;

        private Timer m_Timer;
        private Process m_Process;
        private string m_PopupClassName;

        public bool IsStarted { get; private set; }

        private readonly IXLogger m_Logger;

        private readonly object m_Lock;

        public PopupKiller(IXLogger logger) 
        {
            IsStarted = false;
            m_Logger = logger;

            m_Lock = new object();
        }

        public void Start(Process prc, TimeSpan period, string popupClassName = "#32770")
        {
            if (!IsStarted)
            {
                IsStarted = true;

                m_Process = prc;
                m_PopupClassName = popupClassName;

                var periodMs = (int)period.TotalMilliseconds;

                m_Timer = new Timer(OnTimer, null,
                    periodMs, periodMs);
            }
            else 
            {
                throw new Exception("Already started");
            }
        }

        private void OnTimer(object state)
        {
            KillPopupIfShown();
        }

        private void KillPopupIfShown()
        {
            if (Monitor.TryEnter(m_Lock))
            {
                try
                {
                    foreach (ProcessThread thread in m_Process.Threads)
                    {
                        var callbackProc = new EnumThreadProc(EnumThreadWindowsCallback);
                        EnumThreadWindows(thread.Id, callbackProc, IntPtr.Zero);
                    }
                }
                finally
                {
                    Monitor.Exit(m_Lock);
                }
            }
        }

        private bool EnumThreadWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            var className = new StringBuilder(256);
            GetClassName(hwnd, className, 256);
            
            if (className.ToString() == m_PopupClassName)
            {
                m_Logger.Log($"Killing popup: {hwnd}", LoggerMessageSeverity_e.Debug);
                SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);

                if (IsWindow(hwnd)) 
                {
                    PopupNotClosed?.Invoke(m_Process);
                }

                return false;
            }

            return true;
        }

        public void Dispose() => Stop();

        public void Stop() 
        {
            m_Timer?.Dispose();
            IsStarted = false;
        }
    }
}
