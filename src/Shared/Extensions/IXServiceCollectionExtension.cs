//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Shared.Extensions
{
    public static class IXServiceCollectionExtension
    {
        public static void Populate(this IXServiceCollection svcColl, IComponentContext context) 
        {
            foreach (var reg in context.ComponentRegistry.Registrations)
            {
                var svcType = (reg.Services.First() as Autofac.Core.TypedService).ServiceType;
                svcColl.AddOrReplace(svcType, () => context.Resolve(svcType));
            }
        }
    }
}
