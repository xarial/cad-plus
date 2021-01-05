using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Common.Sw.Services;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.Common.Sw
{
    public static class ContainerBuilderExtension
    {
        public static void UsingCommonSwServices(this IContainerBuilder builder) 
        {
            if (builder is ContainerBuilderWrapper)
            {
                (builder as ContainerBuilderWrapper).Builder.UsingCommonSwServices();
            }
            else
            {
                throw new InvalidCastException($"{typeof(ContainerBuilderWrapper).FullName} is supported");
            }
        }
        
        public static void UsingCommonSwServices(this ContainerBuilder builder)
        {
            builder.RegisterAdapter<IXApplication, ISwApplication>(a => (ISwApplication)a);

            builder.RegisterType<SwMacroRunnerExService>()
                .As<IMacroRunnerExService>();

            builder.RegisterType<SwMacroFileFilterProvider>()
                .As<IMacroFileFilterProvider>();
        }
    }
}
