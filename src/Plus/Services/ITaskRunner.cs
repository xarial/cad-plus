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
using System.Threading;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface ITaskRunnerFactory 
    {
        ITaskRunner Create();
    }

    public interface ITaskRunner : IDisposable
    {
        Task RunAsync(Action action, CancellationToken cancellationToken);
        Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken);
    }
}
