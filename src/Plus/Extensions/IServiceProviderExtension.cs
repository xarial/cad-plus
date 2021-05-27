//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Extensions
{
    public static class IServiceProviderExtension
    {
        public static TSvc GetService<TSvc>(this IServiceProvider svcProv)
            => (TSvc)svcProv.GetService(typeof(TSvc));

        public static TSvc GetService<TSvc>(this IServiceContainer svcProv, string name)
            => (TSvc)svcProv.GetService(typeof(TSvc), name);
    }
}
