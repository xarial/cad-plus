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
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared
{
    public class ContainerBuilderWrapper : IContainerBuilder
    {
        internal event Action<IContainer> ContainerBuild;

        public ContainerBuilder Builder { get; }

        private IServiceContainer m_Container;

        public ContainerBuilderWrapper(ContainerBuilder builder) 
        {
            Builder = builder;
        }

        public void Register<TImplementer, TService>() where TImplementer : TService
            => Builder.RegisterType<TImplementer>().As<TService>();

        public IServiceContainer Build()
        {
            if (m_Container == null)
            {
                var cont = Builder.Build();
                ContainerBuild?.Invoke(cont);

                m_Container = new ServiceProvider(cont);

                return m_Container;
            }
            else 
            {
                throw new Exception("Contained already built");
            }
        }

        public void RegisterInstance<TInstance, TService>(TInstance inst)
            where TInstance : class, TService
        { 
            Builder.RegisterInstance(inst)
                .AsSelf()
                .As<TService>();
        }

        public void Register<TImplementer, TService>(string name)
            where TImplementer : TService
            => Builder.RegisterType<TImplementer>().Named<TService>(name);

        public void Register<TService>(Func<IServiceContainer, TService> provider)
            => Builder.Register(c => provider.Invoke(m_Container));

        public void RegisterAdapter<TFrom, TTo>(Func<TFrom, TTo> adapter)
            where TTo : TFrom
            => Builder.RegisterAdapter<TFrom, TTo>(adapter);
    }
}
