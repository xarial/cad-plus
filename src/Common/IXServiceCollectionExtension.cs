using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;

namespace Xarial.CadPlus.Common
{
    public static class IXServiceCollectionExtension
    {
        public static void Populate(this IXServiceCollection svcColl, IComponentContext context) 
        {
            foreach (var reg in context.ComponentRegistry.Registrations)
            {
                var svcType = (reg.Services.First() as Autofac.Core.TypedService).ServiceType;
                svcColl.AddOrReplace(svcType,
                    () => context.Resolve(svcType));
            }
        }
    }
}
