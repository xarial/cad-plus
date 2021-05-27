//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.XCad;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Delegates
{
    public delegate void ProcessBatchInputDelegate(IXApplication app,
        ICadApplicationInstanceProvider instProvider,
        List<IXDocument> input);
}
