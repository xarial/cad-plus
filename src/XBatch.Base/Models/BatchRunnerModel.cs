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
using Xarial.XCad;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base.Models
{
    public interface IBatchRunnerModel 
    {
        ObservableCollection<string> RecentFiles { get; }
        IXVersion[] InstalledVersions { get; }

        FileFilter[] InputFilesFilter { get; }
        FileFilter[] MacroFilesFilter { get; }

        void SaveJobToFile(BatchJob job, string filePath);
        BatchJob LoadJobFromFile(string filePath);
        BatchJob CreateNewJobDocument();

        IXVersion ParseVersion(string id);

        IBatchRunJobExecutor CreateExecutor(BatchJob job);

        string GetVersionId(IXVersion value);
    }

    public class BatchRunnerModel : IBatchRunnerModel
    {
        private readonly IApplicationProvider m_AppProvider;

        private readonly IRecentFilesManager m_RecentFilesMgr;

        public ObservableCollection<string> RecentFiles { get; }

        private readonly Func<BatchJob, IBatchRunJobExecutor> m_ExecFact;

        public BatchRunnerModel(IApplicationProvider appProvider, IRecentFilesManager recentFilesMgr, 
            IMacroFileFilterProvider macroFilterProvider, Func<BatchJob, IBatchRunJobExecutor> execFact) 
        {
            m_AppProvider = appProvider;
            m_RecentFilesMgr = recentFilesMgr;
            RecentFiles = new ObservableCollection<string>(m_RecentFilesMgr.RecentFiles);

            InputFilesFilter = appProvider.InputFilesFilter;
            MacroFilesFilter = macroFilterProvider.GetSupportedMacros()
                .Union(new FileFilter[] { FileFilter.AllFiles }).ToArray();

            m_ExecFact = execFact;

            InstalledVersions = m_AppProvider.GetInstalledVersions().ToArray();

            if (!InstalledVersions.Any()) 
            {
                throw new UserMessageException("Failed to detect any installed version of the host application");
            }
        }

        public FileFilter[] InputFilesFilter { get; }

        public FileFilter[] MacroFilesFilter { get; }

        public IXVersion[] InstalledVersions { get; }

        public IBatchRunJobExecutor CreateExecutor(BatchJob job) => m_ExecFact.Invoke(job);

        public BatchJob CreateNewJobDocument() => new BatchJob();

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

        public IXVersion ParseVersion(string id) => m_AppProvider.ParseVersion(id);

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

        public string GetVersionId(IXVersion value) => m_AppProvider.GetVersionId(value);
    }
}
