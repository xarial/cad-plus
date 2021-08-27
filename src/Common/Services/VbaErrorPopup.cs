//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Services
{
    /// <summary>
    /// This class allows to handle the VBA exception (parse error text and close the dialog)
    /// </summary>
    public class VbaErrorPopup : IDisposable
    {
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

        public bool IsVbaErrorPopup { get; private set; }
        public string ErrorText { get; private set; }

        private IntPtr m_Label;
        private IntPtr m_ContinueButton;
        private IntPtr m_EndButton;
        private IntPtr m_DebugButton;
        private IntPtr m_HelpButton;

        private int m_CurControlIndex;

        public VbaErrorPopup(IntPtr hWnd)
        {
            IsVbaErrorPopup = false;

            var wndCaption = GetText(hWnd);

            if (wndCaption == "Microsoft Visual Basic")
            {
                EnumChildWindows(hWnd, EnumWindow, IntPtr.Zero);

                const int NUMBER_OF_CONTROLS = 5;

                if (m_CurControlIndex == NUMBER_OF_CONTROLS
                    && !IntPtr.Zero.Equals(m_Label)
                    && !IntPtr.Zero.Equals(m_ContinueButton)
                    && !IntPtr.Zero.Equals(m_EndButton)
                    && !IntPtr.Zero.Equals(m_DebugButton)
                    && !IntPtr.Zero.Equals(m_HelpButton))
                {
                    IsVbaErrorPopup = true;
                    ErrorText = GetText(m_Label).Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                }
            }
        }

        public void Close()
        {
            if (IsVbaErrorPopup)
            {
                ClickButton(m_EndButton);
            }
            else
            {
                throw new Exception("Specified window is not a VBA Exception");
            }
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            var className = GetClassName(hWnd);

            if (className == "Static" && m_CurControlIndex == 0)
            {
                m_Label = hWnd;
            }
            else if (className == "Button" && m_CurControlIndex == 1)//"&Continue"
            {
                m_ContinueButton = hWnd;
            }
            else if (className == "Button" && m_CurControlIndex == 2)//"&End"
            {
                m_EndButton = hWnd;
            }
            else if (className == "Button" && m_CurControlIndex == 3)//"&Debug"
            {
                m_DebugButton = hWnd;
            }
            else if (className == "Button" && m_CurControlIndex == 4)//"&Help"
            {
                m_HelpButton = hWnd;
            }

            m_CurControlIndex++;
            return true;
        }

        private string GetText(IntPtr hWnd)
        {
            var text = new StringBuilder(GetWindowTextLength(hWnd) + 1);

            GetWindowText(hWnd, text, text.Capacity);

            return text.ToString();
        }

        private string GetClassName(IntPtr hWnd)
        {
            var className = new StringBuilder();
            GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }

        private void ClickButton(IntPtr hWnd)
        {
            const int BM_CLICK = 0x00F5;
            SendMessage(hWnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        public void Dispose()
        {
        }
    }
}
