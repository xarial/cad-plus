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
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.Plus.Shared
{
    public class ContainerBuilderWrapper : IContainerBuilder
    {
        internal event Action<IContainer> ContainerBuild;

        public ContainerBuilder Builder { get; }

        public ContainerBuilderWrapper(ContainerBuilder builder) 
        {
            Builder = builder;
        }

        public void Register<TImplementer, TService>() where TImplementer : TService
            => Builder.RegisterType<TImplementer>().As<TService>();

        public IServiceProvider Build()
        {
            var cont = Builder.Build();
            ContainerBuild?.Invoke(cont);
            return new ServiceProvider(cont);
        }

        public void RegisterInstance<TInstance, TService>(TInstance inst)
            where TInstance : class, TService
        { 
            Builder.RegisterInstance(inst)
                .AsSelf()
                .As<TService>();
        }
    }
}
