﻿//*********************************************************************
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
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Plus.Extensions
{
    public static class ExceptionExtension
    {
        public static string ParseUserError(this Exception ex, string genericError = "Generic error")
            => ex.ParseUserError(out _, genericError, UserException.AdditionalUserExceptions);
    }
}
