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
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Services;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public interface IBatchRunnerModel 
    {
        ObservableCollection<string> RecentFiles { get; }
        AppVersionInfo[] InstalledVersions { get; }

        FileFilter[] InputFilesFilter { get; }
        FileFilter[] MacroFilesFilter { get; }

        void SaveJobToFile(BatchJob job, string filePath);
        BatchJob LoadJobFromFile(string filePath);
        BatchJob CreateNewJobDocument();

        AppVersionInfo ParseVersion(string id);

        IBatchRunJobExecutor CreateExecutor(BatchJob job);
    }

    public class BatchRunnerModel : IBatchRunnerModel
    {
        private readonly IApplicationProvider m_AppProvider;

        private readonly IRecentFilesManager m_RecentFilesMgr;

        public ObservableCollection<string> RecentFiles { get; }

        public BatchRunnerModel(IApplicationProvider appProvider, IRecentFilesManager recentFilesMgr) 
        {
            m_AppProvider = appProvider;
            m_RecentFilesMgr = recentFilesMgr;
            RecentFiles = new ObservableCollection<string>(m_RecentFilesMgr.RecentFiles);

            InstalledVersions = m_AppProvider.GetInstalledVersions().ToArray();

            if (!InstalledVersions.Any()) 
            {
                throw new UserMessageException("Failed to detect any installed version of the host application");
            }
        }

        public FileFilter[] InputFilesFilter => m_AppProvider.InputFilesFilter;

        public FileFilter[] MacroFilesFilter => m_AppProvider.MacroFilesFilter;

        public AppVersionInfo[] InstalledVersions { get; }

        public IBatchRunJobExecutor CreateExecutor(BatchJob job) => new BatchRunJobExecutor(job, m_AppProvider);

        public BatchJob CreateNewJobDocument() => new BatchJob();

        public BatchJob LoadJobFromFile(string filePath)
        {
            var svc = new UserSettingsService();

            var batchJob = svc.ReadSettings<BatchJob>(filePath);

            UpdateRecentFiles(filePath);

            return batchJob;
        }

        public AppVersionInfo ParseVersion(string id) => m_AppProvider.ParseVersion(id);

        public void SaveJobToFile(BatchJob job, string filePath)
        {
            var svc = new UserSettingsService();

            svc.StoreSettings(job, filePath);

            UpdateRecentFiles(filePath);
        }

        private void UpdateRecentFiles(string filePath) 
        {
            m_RecentFilesMgr.PushFile(filePath);
            RecentFiles.Clear();
            
            foreach (var recFile in m_RecentFilesMgr.RecentFiles)
            {
                RecentFiles.Add(recFile);
            }
        }
    }
}
