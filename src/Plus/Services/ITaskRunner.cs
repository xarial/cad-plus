﻿using System;
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
        Task Run(Action action, CancellationToken cancellationToken);
        Task<T> Run<T>(Func<T> func, CancellationToken cancellationToken);
    }
}
