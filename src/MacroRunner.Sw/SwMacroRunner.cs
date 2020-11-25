//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.MacroRunner.Sw
{
    [ComVisible(true)]
    [ProgId("CadPlus.MacroRunner.Sw")]
    [Guid("D52774EB-4280-4205-A0CB-E0984665F06A")]
    public class SwMacroRunner : MacroRunnerBase
    {
        protected override IXApplication CastApplication(object app)
        {
            if (app is ISldWorks)
            {
                return SwApplicationFactory.FromPointer((ISldWorks)app);
            }
            else 
            {
                throw new InvalidCastException("Pointer to dispatch is not ISldWorks");
            }
        }

        protected override string CreateMacroSessionId(IXApplication app, IXMacro macro)
            => CreateSessionId(app, macro.Path);

        protected override string GetCurrentMacroSessionId(IXApplication app)
            => CreateSessionId(app, (app as ISwApplication).Sw.GetCurrentMacroPathName());

        private string CreateSessionId(IXApplication app, string macroPath)
            => $"{app.Process.Id}_{macroPath.ToLower()}";
    }
}
