//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
        public Guid Id { get; }

        public CadExtensionApplication(IXExtension ext, Guid appId) 
        {
            Extension = ext;
            Id = appId;
        }
    }
}
