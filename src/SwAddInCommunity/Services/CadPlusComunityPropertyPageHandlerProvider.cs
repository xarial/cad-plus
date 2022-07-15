//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage;

namespace Xarial.CadPlus.AddIn.Sw.Services
{
    [ComVisible(true)]
    public class SwGeneralPropertyManagerPageCommunityHandler : SwPropertyManagerPageHandler
    {
    }

    internal class CadPlusComunityPropertyPageHandlerProvider : IPropertyPageHandlerProvider
    {
        public SwPropertyManagerPageHandler CreateHandler(ISwApplication app, Type handlerType)
            => new SwGeneralPropertyManagerPageCommunityHandler();
    }
}
