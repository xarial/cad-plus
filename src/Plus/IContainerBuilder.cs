//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus
{
    public interface IContainerBuilder
    {
        IServiceContainer Build();
        
        void Register<TImplementer, TService>()
            where TImplementer : TService;

        void Register<TImplementer, TService>(string name)
            where TImplementer : TService;

        void Register<TService>(Func<IServiceContainer, TService> provider);

        void RegisterAdapter<TFrom, TTo>(Func<TFrom, TTo> adapter)
            where TTo : TFrom;

        void RegisterInstance<TInstance, TService>(TInstance inst)
            where TInstance : class, TService;
    }
}
