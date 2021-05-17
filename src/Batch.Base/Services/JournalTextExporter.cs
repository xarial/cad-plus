//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IJournalExporter 
    {
        FileFilter Filter { get; }
        void Export(JobResultJournalVM journal, string filePath);
    }

    public class JournalTextExporter : IJournalExporter
    {
        public FileFilter Filter { get; }

        public JournalTextExporter() 
        {
            Filter = new FileFilter("Text File", "*.txt");
        }

        public void Export(JobResultJournalVM journal, string filePath)
        {
            using (var file = File.CreateText(filePath)) 
            {
                foreach (var line in journal.Output) 
                {
                    file.WriteLine(line);
                }
            }
        }
    }
}
