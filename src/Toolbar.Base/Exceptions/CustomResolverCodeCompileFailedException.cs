//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Microsoft.CodeAnalysis;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.CustomToolbar.Exceptions
{
    public class CustomResolverCodeCompileFailedException : Exception
    {
        public CustomResolverCodeCompileFailedException(IEnumerable<Diagnostic> errors) 
            : base(string.Join(Environment.NewLine, errors))
        {
        }
    }
}
