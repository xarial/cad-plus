//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Threading;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class GenericMessageService : WindowsMessageService
    {
        public GenericMessageService() : base("CAD+ Toolset")
        {
        }
    }
}
