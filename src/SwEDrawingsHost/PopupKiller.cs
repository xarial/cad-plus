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

namespace Xarial.CadPlus.Xport.SwEDrawingsHost
{
    public class PopupKiller : IDisposable
    {
        private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly System.Threading.Timer m_Timer;
        private readonly Process m_Process;
        private readonly string m_PopupClassName;

        public PopupKiller(Process prc) : this(prc, TimeSpan.FromSeconds(1))
        {
        }

        public PopupKiller(Process prc, TimeSpan period, string popupClassName = "#32770")
        {
            m_Process = prc;
            m_PopupClassName = popupClassName;

            var periodMs = (int)period.TotalMilliseconds;

            m_Timer = new System.Threading.Timer(OnTimer, null,
                periodMs, periodMs);
        }

        private void OnTimer(object state)
        {
            KillPopupIfShown();
        }

        private void KillPopupIfShown()
        {
            foreach (ProcessThread thread in m_Process.Threads)
            {
                var callbackProc = new EnumThreadProc(EnumThreadWindowsCallback);
                EnumThreadWindows(thread.Id, callbackProc, IntPtr.Zero);
            }
        }

        private bool EnumThreadWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            var className = new StringBuilder(256);
            GetClassName(hwnd, className, 256);

            Debug.Print(className.ToString());

            if (className.ToString() == m_PopupClassName)
            {
                Debug.Print($"Killing popup");
                SendMessage(hwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            m_Timer.Dispose();
        }
    }
}