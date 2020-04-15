//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using eDrawings.Interop.EModelViewControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xarial.XTools.Xport.Core
{
    public class Exporter : IDisposable
    {
        private readonly EDrawingsHost m_EdrawingsControl;
        private readonly TextWriter m_Logger;
        private readonly IProgress<double> m_ProgressHandler;

        public Exporter(TextWriter logger, IProgress<double> progressHandler = null)
        {
            m_EdrawingsControl = new EDrawingsHost();
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
        }
        
        public async Task Export(ExportOptions opts, CancellationToken token = default) 
        {
            var curTime = DateTime.Now;

            var jobs = ParseOptions(opts);

            var totalJobs = jobs.Sum(j => j.Value.Length);
            int curJob = 0;

            foreach (var job in jobs)
            {
                var file = job.Key;

                try
                {
                    var outFiles = job.Value;

                    m_Logger.WriteLine($"Opening '{file}'...");

                    await m_EdrawingsControl.OpenDocument(file).ConfigureAwait(false);

                    m_Logger.WriteLine($"'{file}' opened. Starting exporting...");

                    foreach (var outFile in outFiles)
                    {
                        if (token.IsCancellationRequested) 
                        {
                            m_Logger.WriteLine($"Cancelled by the user");
                            return;
                        }

                        m_Logger.WriteLine($"Exporting '{file}' to '{outFile}");
                        await ExportFile(outFile, opts.ContinueOnError);
                        
                        m_ProgressHandler?.Report(++curJob / (double)totalJobs);
                    }

                    m_EdrawingsControl.CloseDocument();
                    m_Logger.WriteLine($"Closing '{file}'...");
                }
                catch (Exception ex)
                {
                    m_Logger.WriteLine($"Error while processing '{file}': {ex.Message}");
                    if (!opts.ContinueOnError) 
                    {
                        throw ex;
                    }
                }
            }

            m_Logger.WriteLine($"Exporting completed in {DateTime.Now.Subtract(curTime).ToString(@"hh\:mm\:ss")}");
        }

        private async Task ExportFile(string outFile, bool continueOnError)
        {
            try
            {
                var ext = Path.GetExtension(outFile);

                if (!string.Equals(ext, ".pdf", StringComparison.CurrentCultureIgnoreCase))
                {
                    await m_EdrawingsControl.SaveDocument(outFile).ConfigureAwait(false);
                }
                else
                {
                    await m_EdrawingsControl.PrintToFile(outFile).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                m_Logger.WriteLine($"Error while exporting to '{outFile}': {ex.Message}");

                if (!continueOnError)
                {
                    throw ex;
                }
            }
        }

        private Dictionary<string, string[]> ParseOptions(ExportOptions opts)
        {
            const string EDRW_FORMAT = ".e";

            var outDir = opts.OutputDirectory;

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            if (string.IsNullOrEmpty(outDir))
            {
                outDir = "";
            }

            var filter = opts.Filter;

            if (string.IsNullOrEmpty(filter))
            {
                filter = "*.*";
            }

            var files = new List<string>();

            foreach (var input in opts.Input)
            {
                if (Directory.Exists(input))
                {
                    files.AddRange(Directory.GetFiles(input, filter, SearchOption.AllDirectories).ToList());
                }
                else if (File.Exists(input))
                {
                    files.Add(input);
                }
                else
                {
                    throw new Exception("Specify input file or directory");
                }
            }

            var jobs = new Dictionary<string, string[]>();

            foreach (var file in files)
            {
                var outFiles = new string[opts.Format.Length];
                jobs.Add(file, outFiles);

                for (int i = 0; i < opts.Format.Length; i++)
                {
                    var ext = opts.Format[i];
                    
                    if (!ext.StartsWith("."))
                    {
                        ext = "." + ext;
                    }

                    if (ext.Equals(EDRW_FORMAT, StringComparison.CurrentCultureIgnoreCase))
                    {
                        switch (Path.GetExtension(file).ToLower())
                        {
                            case ".sldprt":
                                ext = ".eprt";
                                break;
                            case ".sldasm":
                                ext = ".easm";
                                break;
                            case ".slddrw":
                                ext = ".edrw";
                                break;
                            default:
                                throw new ArgumentException($"{EDRW_FORMAT} format is only applicable for SOLIDWORKS files");
                        }
                    }

                    outFiles[i] = Path.Combine(!string.IsNullOrEmpty(outDir) ? outDir : Path.GetDirectoryName(file),
                        Path.GetFileNameWithoutExtension(file) + ext);
                }
            }

            return jobs;
        }
        
        public void Dispose()
        {
            m_EdrawingsControl.Dispose();
        }
    }
}
