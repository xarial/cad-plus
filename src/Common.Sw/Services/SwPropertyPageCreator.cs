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
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.CadPlus.Common.Sw.Services
{
    public class SwPropertyPageCreator<THandler> : IPropertyPageCreator
        where THandler : SwPropertyManagerPageHandler, new()
    {
        private readonly ISwAddInEx m_AddIn;

        public SwPropertyPageCreator(ISwAddInEx addIn)
        {
            m_AddIn = addIn;
        }

        public IXPropertyPage<TData> CreatePage<TData>()
            => m_AddIn.CreatePage<TData, THandler>();
    }
}
