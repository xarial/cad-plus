//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
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

    public interface IPopupKillerFactory
    {
        IPopupKiller Create();
    }
}
