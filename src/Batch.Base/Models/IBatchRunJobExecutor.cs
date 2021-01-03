using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Batch.Base.Models
{
    public interface IBatchRunJobExecutor
    {
        event Action<IJobItem[], DateTime> JobSet;
        event Action<TimeSpan> JobCompleted;
        event Action<IJobItem, bool> ProgressChanged;
        event Action<string> Log;

        Task<bool> ExecuteAsync();
        void Cancel();
    }
}
