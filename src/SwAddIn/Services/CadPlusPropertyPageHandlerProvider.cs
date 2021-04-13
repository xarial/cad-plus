using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage;

namespace Xarial.CadPlus.AddIn.Sw.Services
{
    internal class CadPlusPropertyPageHandlerProvider : IPropertyPageHandlerProvider
    {
        public SwPropertyManagerPageHandler CreateHandler(ISldWorks app, Type handlerType)
            => new SwGeneralPropertyManagerPageHandler();
    }
}
