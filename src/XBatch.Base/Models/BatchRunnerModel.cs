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
using Xarial.CadPlus.XBatch.Base.Core;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public class BatchRunnerModel
    {
        public event Action<double> ProgressChanged;
        public event Action<string> Log;

        private CancellationTokenSource m_CurrentCancellationToken;

        private readonly IApplicationProvider m_AppProvider;

        public BatchRunnerModel(IApplicationProvider appProvider) 
        {
            m_AppProvider = appProvider;
            InstalledVersions = m_AppProvider.GetInstalledVersions().ToArray();
        }

        public AppVersionInfo[] InstalledVersions { get; }

        public async Task BatchRun(BatchRunnerOptions opts)
        {
            m_CurrentCancellationToken = new CancellationTokenSource();

            var logWriter = new LogWriter();
            var prgHander = new ProgressHandler();

            logWriter.Log += OnLog;
            prgHander.ProgressChanged += OnProgressChanged;

            try
            {
                using (var batchRunner = new BatchRunner(m_AppProvider, logWriter, prgHander))
                {
                    await batchRunner.BatchRun(opts, m_CurrentCancellationToken.Token).ConfigureAwait(false);
                }
            }
            finally
            {
                logWriter.Log -= OnLog;
                prgHander.ProgressChanged -= OnProgressChanged;
            }
        }

        public void Cancel()
        {
            m_CurrentCancellationToken.Cancel();
        }

        private void OnLog(string line)
        {
            Log?.Invoke(line);
        }

        private void OnProgressChanged(double prg)
        {
            ProgressChanged?.Invoke(prg);
        }
    }
}
