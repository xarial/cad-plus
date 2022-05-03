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
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.CustomToolbar.Helpers
{
    public static class MacroScopeHelper
    {
        public static bool IsInScope(this MacroScope_e scope, IXApplication app)
        {
            if (app.Documents.Active == null && scope.HasFlag(MacroScope_e.Application))
            {
                return true;
            }
            else if (app.Documents.Active is IXPart && scope.HasFlag(MacroScope_e.Part))
            {
                return true;
            }
            else if (app.Documents.Active is IXAssembly && scope.HasFlag(MacroScope_e.Assembly))
            {
                return true;
            }
            else if (app.Documents.Active is IXDrawing && scope.HasFlag(MacroScope_e.Drawing))
            {
                return true;
            }

            return false;
        }
    }
}
