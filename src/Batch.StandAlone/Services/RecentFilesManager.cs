//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Hosts;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IRecentFilesManager 
    {
        void RemoveFile(string filePath);
        void PushFile(string filePath);
        IEnumerable<string> RecentFiles { get; }
    }

    public class RecentFilesManager : IRecentFilesManager
    {
        private const int MAX_FILES = 5;

        public IEnumerable<string> RecentFiles => m_RecentFiles;

        private List<string> m_RecentFiles;

        private readonly string m_RecentFilesPath = Path.Combine
            (Locations.AppDirectoryPath, "batchplusrecentfiles.txt");

        private readonly IXLogger m_Logger;

        public void PushFile(string filePath)
        {
            m_RecentFiles.Insert(0, filePath);

            var existingIndex = m_RecentFiles.FindIndex(1, x => string.Equals(x, filePath, StringComparison.CurrentCultureIgnoreCase));

            if (existingIndex != -1)
            {
                m_RecentFiles.RemoveAt(existingIndex);
            }

            TruncateList();

            TrySaveRecentFiles();
        }

        private void TruncateList()
        {
            if (m_RecentFiles.Count > MAX_FILES)
            {
                m_RecentFiles.RemoveRange(MAX_FILES, m_RecentFiles.Count - MAX_FILES);
            }
        }

        public RecentFilesManager(IXLogger logger) 
        {
            m_Logger = logger;

            m_RecentFiles = new List<string>(TryLoadRecentFiles());
            TruncateList();
        }

        private string[] TryLoadRecentFiles() 
        {
            string[] recentFiles = null;

            try
            {
                if (File.Exists(m_RecentFilesPath)) 
                {
                    recentFiles = File.ReadAllLines(m_RecentFilesPath);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return recentFiles ?? new string[0];
        }

        private void TrySaveRecentFiles() 
        {
            try
            {
                var dir = Path.GetDirectoryName(m_RecentFilesPath);

                if (!Directory.Exists(dir)) 
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllLines(m_RecentFilesPath, m_RecentFiles);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }

        public void RemoveFile(string filePath)
        {
            var existingIndex = m_RecentFiles.FindIndex(1, x => string.Equals(x, filePath, StringComparison.CurrentCultureIgnoreCase));

            if (existingIndex != -1)
            {
                m_RecentFiles.RemoveAt(existingIndex);
            }

            TruncateList();

            TrySaveRecentFiles();
        }
    }
}
