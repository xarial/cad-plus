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
        public static void Populate(this IXServiceCollection svcColl, IContainer cont) 
        {
            foreach (var reg in cont.ComponentRegistry.Registrations)
            {
                var svcType = (reg.Services.First() as Autofac.Core.TypedService).ServiceType;
                svcColl.AddOrReplace(svcType,
                    () => cont.Resolve(svcType));
            }
        }
    }
}
