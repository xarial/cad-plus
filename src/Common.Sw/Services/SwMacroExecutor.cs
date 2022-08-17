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
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.CadPlus.Common.Sw.Services
{
    [CadSpecificService(CadApplicationIds.SolidWorks)]
    public class SwMacroExecutor : MacroExecutor
    {
        protected override string MacroRunnerProgId => "CadPlus.MacroRunner.Sw";
        protected override object GetAppDispatch(IXApplication app) => ((ISwApplication)app).Sw;
        protected override object GetDocumentDispatch(IXDocument doc) => ((ISwDocument)doc).Model;

        public SwMacroExecutor(IXCadMacroProvider xCadMacroProvider, IXLogger logger) : base(xCadMacroProvider, logger)
        {
        }
    }
}
