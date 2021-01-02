using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Hosts;

namespace Xarial.CadPlus.XBatch.Base.Services
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

        public RecentFilesManager() 
        {
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
            catch 
            {
            }

            return recentFiles ?? new string[0];
        }

        private void TrySaveRecentFiles() 
        {
            try
            {
                File.WriteAllLines(m_RecentFilesPath, m_RecentFiles);
            }
            catch 
            {
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
