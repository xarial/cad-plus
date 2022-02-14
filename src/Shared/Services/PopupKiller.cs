//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
    public delegate void PopupNotClosedDelegate(Process prc, IntPtr hWnd);
    public delegate void ShouldClosePopupDelegate(Process prc, IntPtr hWnd, ref bool close);

    public interface IPopupKiller : IDisposable
    {
        event PopupNotClosedDelegate PopupNotClosed;
        event ShouldClosePopupDelegate ShouldClosePopup;

        bool IsStarted { get; }
        void Start(Process prc, TimeSpan period, string popupClassName = "#32770");
        void Stop();
    }

    public class PopupKiller : IPopupKiller
    {
        #region Windows API

        private delegate bool EnumThreadProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        #endregion

        public event PopupNotClosedDelegate PopupNotClosed;
        public event ShouldClosePopupDelegate ShouldClosePopup;

        private Timer m_Timer;
        private Process m_Process;
        private string m_PopupClassName;

        public bool IsStarted { get; private set; }

        private readonly IXLogger m_Logger;

        private readonly object m_Lock;

        private IntPtr m_CurrentModalPopupHwnd;

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
                var curModalPopupHwnd = m_CurrentModalPopupHwnd;

                m_CurrentModalPopupHwnd = IntPtr.Zero;

                if (!IntPtr.Zero.Equals(curModalPopupHwnd) && IsWindow(curModalPopupHwnd))
                {
                    PopupNotClosed?.Invoke(m_Process, curModalPopupHwnd);
                }
                else
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
        }

        private bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            
            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            
            if (className.ToString() == m_PopupClassName)
            {
                if (IsWindow(hWnd) && IsModalPopup(hWnd))
                {
                    var close = true;

                    ShouldClosePopup?.Invoke(m_Process, hWnd, ref close);

                    if (close)
                    {
                        m_Logger.Log($"Killing popup: {hWnd}", LoggerMessageSeverity_e.Debug);
                        SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, 0);

                        if (IsWindow(hWnd))
                        {
                            //windows closing may take time. Remember the hWnd and check if still active in next timer tick
                            m_CurrentModalPopupHwnd = hWnd;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        private bool IsModalPopup(IntPtr hwnd) 
        {
            const uint GW_OWNER = 4;
            const int GWL_STYLE = -16;
            const uint WS_DISABLED = 0x8000000;
            const uint GA_PARENT = 1;

            return GetAncestor(hwnd, GA_PARENT) == GetDesktopWindow()
                && (GetWindowLong(GetWindow(hwnd, GW_OWNER), GWL_STYLE) & WS_DISABLED) != 0;
        }

        public void Dispose() => Stop();

        public void Stop() 
        {
            m_Timer?.Dispose();
            IsStarted = false;
        }
    }
}
