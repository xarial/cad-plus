//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Services;
using Xarial.XCad;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public interface IBatchRunnerModel 
    {
        ObservableCollection<string> RecentFiles { get; }
        
        void SaveJobToFile(BatchJob job, string filePath);
        BatchJob LoadJobFromFile(string filePath);
        BatchJob CreateNewJobDocument(string appId);
    }

    public class BatchRunnerModel : IBatchRunnerModel
    {
        private readonly IRecentFilesManager m_RecentFilesMgr;

        public ObservableCollection<string> RecentFiles { get; }
        
        public BatchRunnerModel(IRecentFilesManager recentFilesMgr)
        {
            m_RecentFilesMgr = recentFilesMgr;
            RecentFiles = new ObservableCollection<string>(m_RecentFilesMgr.RecentFiles);
        }

        public BatchJob CreateNewJobDocument(string appId)
            => new BatchJob()
            {
                ApplicationId = appId
            };

        public BatchJob LoadJobFromFile(string filePath)
        {
            try
            {
                var batchJob = BatchJob.FromFile(filePath);

                AppendRecentFiles(filePath);

                return batchJob;
            }
            catch 
            {
                m_RecentFilesMgr.RemoveFile(filePath);
                UpdateRecentFiles();
                throw;
            }
        }
        
        public void SaveJobToFile(BatchJob job, string filePath)
        {
            var svc = new UserSettingsService();

            svc.StoreSettings(job, filePath);

            AppendRecentFiles(filePath);
        }

        private void AppendRecentFiles(string filePath)
        {
            m_RecentFilesMgr.PushFile(filePath);
            UpdateRecentFiles();
        }

        private void UpdateRecentFiles()
        {
            RecentFiles.Clear();

            foreach (var recFile in m_RecentFilesMgr.RecentFiles)
            {
                RecentFiles.Add(recFile);
            }
        }
    }
}
