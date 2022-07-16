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
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Common.Services
{
    /// <summary>
    /// Executes the batch running job and tracks the progress
    /// </summary>
    /// <remarks>This is a transient service and should be created and disposed once per the batch run</remarks>
    public interface IBatchRunJobExecutor : IDisposable
    {
        event Action<IJobItem[], DateTime> JobSet;
        event Action<TimeSpan> JobCompleted;
        event Action<string> StatusChanged;
        event Action<IJobItem, double, bool> ProgressChanged;
        event Action<string> Log;

        bool Execute(CancellationToken cancellationToken);
        Task<bool> ExecuteAsync(CancellationToken cancellationToken);
    }
}
