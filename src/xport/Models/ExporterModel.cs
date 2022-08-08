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
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Xport.Core;
using Xarial.CadPlus.Xport.ViewModels;

namespace Xarial.CadPlus.Xport.Models
{
    public interface IExporterModel 
    {
        event Action<double> ProgressChanged;
        event Action<string> Log;
        Task Export(ExportOptions opts);
        void Cancel();
    }

    public class ExporterModel : IExporterModel
    {
        public event Action<double> ProgressChanged;
        public event Action<string> Log;

        private CancellationTokenSource m_CurrentCancellationToken;

        private readonly IJobProcessManager m_JobMgr;

        public ExporterModel(IJobProcessManager jobMgr) 
        {
            m_JobMgr = jobMgr;
        }

        public async Task Export(ExportOptions opts) 
        {
            m_CurrentCancellationToken = new CancellationTokenSource();

            using (var exporter = new Exporter(m_JobMgr, opts))
            {
                exporter.ItemProcessed += OnItemProcessed;
                exporter.Log += OnLog;
                await exporter.ExecuteAsync(m_CurrentCancellationToken.Token).ConfigureAwait(false);
            }
        }

        private void OnItemProcessed(IBatchJobBase sender, IJobItem item, double progress, bool result)
        {
            ProgressChanged?.Invoke(progress);
        }

        public void Cancel()
        {
            m_CurrentCancellationToken.Cancel();
        }

        private void OnLog(IBatchJobBase sender, string line)
        {
            Log?.Invoke(line);
        }
    }
}
