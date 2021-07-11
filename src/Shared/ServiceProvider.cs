//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Autofac;
using System;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared
{
    public class ServiceProvider : IServiceContainer, IDisposable
    {
        public IComponentContext Context { get; }

        public ServiceProvider(IComponentContext container) 
        {
            Context = container;
        }

        public object GetService(Type serviceType)
            => Context.Resolve(serviceType);

        public object GetService(Type serviceType, string name)
            => Context.ResolveNamed(name, serviceType);

        public void Dispose() 
        {
            if (Context is IDisposable) 
            {
                (Context as IDisposable).Dispose();
            }
        }
    }
}
