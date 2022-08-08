//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IJournalExporter 
    {
        FileFilter Filter { get; }
        void Export(JobResultBaseVM jobResult, string filePath);
    }

    public class JournalTextExporter : IJournalExporter
    {
        public FileFilter Filter { get; }

        public JournalTextExporter() 
        {
            Filter = new FileFilter("Text File", "*.txt");
        }

        public void Export(JobResultBaseVM jobResult, string filePath)
        {
            using (var file = File.CreateText(filePath)) 
            {
                foreach (var line in jobResult.Output) 
                {
                    file.WriteLine(line);
                }
            }
        }
    }
}
