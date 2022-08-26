//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
        public static TService GetService<TService>(this IServiceProvider svcProvider)
            where TService : class => (TService)svcProvider.GetService(typeof(TService));
    }
}
