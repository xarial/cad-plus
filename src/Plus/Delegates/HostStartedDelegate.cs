﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IHost.Started"/> event
    /// </summary>
    /// <param name="parentWnd">Parent window of this application</param>
    public delegate void HostStartedDelegate(IntPtr parentWnd);
}
