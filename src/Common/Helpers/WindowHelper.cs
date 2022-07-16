//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Helpers
{
    public static class WindowHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        public static void TrySendWindowBackground(IntPtr hWnd) 
        {
            try
            {
                if (!IntPtr.Zero.Equals(hWnd))
                {
                    const int HWND_BOTTOM = 1;
                    const int SWP_NOACTIVATE = 0x0010;

                    SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOACTIVATE);
                }
            }
            catch 
            {
            }
        }
    }
}
