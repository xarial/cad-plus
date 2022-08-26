//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class CadParentWindowProvider : IParentWindowProvider
    {
        public Window Window => null;

        public IntPtr Handle { get; }

        public CadParentWindowProvider(IXApplication app)
        {
            Handle = app.WindowHandle;
        }
    }
}
