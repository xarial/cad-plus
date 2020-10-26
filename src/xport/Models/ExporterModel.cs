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
using Xarial.CadPlus.Common.Services;
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

        private int m_ProcessedFiles;
        private int m_TotalFiles;

        public async Task Export(ExportOptions opts) 
        {
            m_CurrentCancellationToken = new CancellationTokenSource();

            var logWriter = new LogWriter();
            var prgHander = new ProgressHandler();

            logWriter.Log += OnLog;
            prgHander.JobScopeSet += OnJobScopeSet;
            prgHander.ProgressChanged += OnProgressChanged;

            try
            {
                using (var exporter = new Exporter(logWriter, prgHander))
                {
                    await exporter.Export(opts, m_CurrentCancellationToken.Token).ConfigureAwait(false);
                }
            }
            finally
            {
                logWriter.Log -= OnLog;
                prgHander.ProgressChanged -= OnProgressChanged;
            }
        }

        private void OnJobScopeSet(IJobItem[] files, DateTime startTime)
        {
            m_ProcessedFiles = 0;
            m_TotalFiles = files.Length;
        }

        public void Cancel()
        {
            m_CurrentCancellationToken.Cancel();
        }

        private void OnLog(string line)
        {
            Log?.Invoke(line);
        }

        private void OnProgressChanged(IJobItem file, bool res)
        {
            m_ProcessedFiles++;

            ProgressChanged?.Invoke(m_ProcessedFiles / (double)m_TotalFiles);
        }
    }
}
