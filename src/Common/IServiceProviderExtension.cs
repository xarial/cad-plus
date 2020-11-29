using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common
{
    public static class IServiceProviderExtension
    {
        public static void RegisterFromServiceProvider<TSvc>(this ContainerBuilder builder, IServiceProvider svcProv)
            where TSvc : class
            => builder.RegisterInstance((TSvc)svcProv.GetService(typeof(TSvc))).As<TSvc>();
    }
}
