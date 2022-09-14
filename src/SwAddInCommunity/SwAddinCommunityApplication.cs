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
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Shared;
using Xarial.XCad.Extensions;

namespace Xarial.CadPlus.AddIn.Sw
{
    public class SwAddinCommunityApplication : CadExtensionApplication, ISwAddInApplication
    {
        public SwAddinCommunityApplication(IXExtension ext) : base(ext)
        {
        }
    }
}
