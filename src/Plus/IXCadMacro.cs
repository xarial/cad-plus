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
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus
{
    public interface IXCadMacro
    {
        void Run(IXApplication app, IXDocument doc, IXLogger logger, string[] args);
    }
}
