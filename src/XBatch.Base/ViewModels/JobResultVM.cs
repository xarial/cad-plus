using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultVM : IJobResult
    {
        IJobResultLog IJobResult.Log => Log;
        IJobResultSummary IJobResult.Summary => Summary;

        public JobResultLogVM Log { get; }
        public JobResultSummaryVM Summary { get; }

        public string Name { get; }

        public JobResultVM(string name)
        {
            Name = name;
            Summary = new JobResultSummaryVM($"Summary of {name}");
            Log = new JobResultLogVM();
            Log.Output = $"Output of {name}";
        }
    }
}
