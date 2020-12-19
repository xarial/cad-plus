//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Common.Services
{
    public interface IMacroFileFilterProvider
    {
        FileFilter[] GetSupportedMacros();
    }
}
