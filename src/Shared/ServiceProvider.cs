﻿//*********************************************************************
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