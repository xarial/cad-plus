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
        public static void UsingCommonSwServices(this IXServiceCollection svc, ISwApplication app)
            => UsingCommonSwServices(svc, () => app);

        public static void UsingCommonSwServices(this IXServiceCollection svc, Func<ISwApplication> appFact)
        {
            var app = appFact.Invoke();

            svc.AddOrReplace<IMacroRunnerExService>(() => new SwMacroRunnerExService(app));
            svc.AddOrReplace<IMacroFileFilterProvider, SwMacroFileFilterProvider>();
        }
    }
}
