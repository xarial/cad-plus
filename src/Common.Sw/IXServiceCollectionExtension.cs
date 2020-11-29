using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Common.Sw.Services;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.Common.Sw
{
    public static class IXServiceCollectionExtension
    {
        public static void UsingCommonSwServices(this ContainerBuilder svc)
        {
            svc.RegisterAdapter<IXApplication, ISwApplication>(a => (ISwApplication)a);

            svc.RegisterType<SwMacroRunnerExService>()
                .As<IMacroRunnerExService>();

            svc.RegisterType<SwMacroFileFilterProvider>()
                .As<IMacroFileFilterProvider>();
        }
    }
}
