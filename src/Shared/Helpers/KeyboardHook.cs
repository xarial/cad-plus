//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xarial.CadPlus.Plus.Shared.Helpers
{
    public class KeyboardHook : IDisposable
    {
        public event Action<KeyboardHook, Keys> KeyDown;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private readonly IntPtr m_HookId;
        private readonly HookProc m_HookCallback;
        private readonly Process m_Proc;

        public KeyboardHook(Process proc)
        {
            m_Proc = proc;
            m_HookCallback = HookCallback;

            using (ProcessModule curModule = m_Proc.MainModule)
            {
                m_HookId = SetWindowsHookEx(WH_KEYBOARD_LL, m_HookCallback,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var vkCode = (Keys)Marshal.ReadInt32(lParam);
                KeyDown?.Invoke(this, vkCode);
            }

            return CallNextHookEx(m_HookId, nCode, wParam, lParam);
        }

        public void Dispose()
            => UnhookWindowsHookEx(m_HookId);
    }
}
