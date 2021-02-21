using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.CadPlus.AddIn.Sw
{
    public class SwPropertyPageCreator<THandler> : IPropertyPageCreator
        where THandler : SwPropertyManagerPageHandler, new()
    {
        private readonly ISwAddInEx m_AddIn;

        public SwPropertyPageCreator(ISwAddInEx addIn)
        {
            m_AddIn = addIn;
        }

        public IXPropertyPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null)
            => m_AddIn.CreatePage<TData, THandler>(createDynCtrlHandler);
    }
}
