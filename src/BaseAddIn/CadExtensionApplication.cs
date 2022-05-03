//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Extensions;
using Xarial.CadPlus.Plus.Applications;

namespace Xarial.CadPlus.AddIn.Base
{
    public class CadExtensionApplication : ICadExtensionApplication
    {
        public IXExtension Extension { get; }

        public CadExtensionApplication(IXExtension ext) 
        {
            Extension = ext;
        }
    }
}
