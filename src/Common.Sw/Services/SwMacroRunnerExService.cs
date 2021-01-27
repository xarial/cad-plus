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
using Xarial.CadPlus.Common.Services;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.CadPlus.Common.Sw.Services
{
    public class SwMacroRunnerExService : MacroRunnerExService
    {
        protected override string MacroRunnerProgId => "CadPlus.MacroRunner.Sw";
        protected override object GetAppDispatch(IXApplication app) => ((ISwApplication)app).Sw;
        protected override object GetDocumentDispatch(IXDocument doc) => ((ISwDocument)doc).Model;
    }
}
