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
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Shared.Extensions
{
    public static class TaskEx
    {
        public static Task Run(Action action, TaskScheduler scheduler)
            => Task.Factory.StartNew(action,
                CancellationToken.None, TaskCreationOptions.None, scheduler);

        public static Task<T> Run<T>(Func<T> func, TaskScheduler scheduler)
            => Task.Factory.StartNew(func,
                CancellationToken.None, TaskCreationOptions.None, scheduler);
    }
}
