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
using Xarial.CadPlus.XBatch.Base.Core;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public class BatchRunnerModel
    {
        public event Action<double> ProgressChanged;
        public event Action<string> Log;

        public Task BatchRun(BatchRunnerOptions opts)
        {
            return Task.CompletedTask;
        }

        public void Cancel()
        {
        }
    }
}
