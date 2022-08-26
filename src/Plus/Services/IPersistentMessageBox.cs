//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IPersistentMessageBoxFactory 
    {
        IPersistentMessageBox Create(IParentWindowProvider windowProvider, string caption, string msg, Image icon);
    }

    public interface IPersistentMessageBox : IDisposable
    {
        void ShowAppendBody(string msg);
    }
}
