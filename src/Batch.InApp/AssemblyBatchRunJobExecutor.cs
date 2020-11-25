using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Batch.InApp
{
    public class AssemblyBatchRunJobExecutor : IBatchRunJobExecutor
    {
        public event Action<IJobItem[], DateTime> JobSet;
        public event Action<TimeSpan> JobCompleted;
        public event Action<IJobItem, bool> ProgressChanged;
        public event Action<string> Log;

        private readonly IXComponent[] m_Comps;

        internal AssemblyBatchRunJobExecutor(IXComponent[] components) 
        {
            m_Comps = components;
        }

        public void Cancel()
        {
        }

        public Task<bool> ExecuteAsync()
        {
            foreach (var comp in m_Comps) 
            {
                IXDocument doc = null;

                if (comp.IsResolved)
                {
                    doc = comp.Document;
                }
                else 
                {
                    //TODO: load document
                }
            }

            return Task.FromResult(false);
        }
    }
}
