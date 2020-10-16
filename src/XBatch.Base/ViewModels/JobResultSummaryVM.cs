using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultSummaryVM : IJobResultSummary
    {
        public string Message { get; set; }

        public JobResultSummaryVM(string msg)
        {
            Message = msg;
        }
    }
}
