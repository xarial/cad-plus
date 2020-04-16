//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xarial.XTools.Xport.Core
{
    public class Exporter : IDisposable
    {
        private readonly TextWriter m_Logger;
        private readonly IProgress<double> m_ProgressHandler;

        public Exporter(TextWriter logger, IProgress<double> progressHandler = null)
        {
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
        }
        
        public async Task Export(ExportOptions opts, CancellationToken token = default) 
        {
            m_Logger.WriteLine($"Exporting Started");

            var curTime = DateTime.Now;

            var jobs = ParseOptions(opts);

            var totalJobs = jobs.Sum(j => j.Value.Length);
            int curJob = 0;

            foreach (var job in jobs)
            {
                var file = job.Key;

                var outFiles = job.Value;

                foreach (var outFile in outFiles)
                {
                    try
                    {
                        var desFile = outFile;

                        int index = 0;

                        while (File.Exists(desFile))
                        {
                            var outDir = Path.GetDirectoryName(outFile);
                            var fileName = Path.GetFileNameWithoutExtension(outFile);
                            var ext = Path.GetExtension(outFile);

                            fileName = $"{fileName} ({++index})";

                            desFile = Path.Combine(outDir, fileName + ext);
                        }

                        if (token.IsCancellationRequested)
                        {
                            m_Logger.WriteLine($"Cancelled by the user");
                            return;
                        }

                        var prcStartInfo = new ProcessStartInfo()
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            FileName = typeof(StandAloneExporter.Program).Assembly.Location,
                            Arguments = $"\"{file}\" \"{outFile}\""
                        };

                        var res = await StartWaitProcessAsync(prcStartInfo, token).ConfigureAwait(false);

                        if (!res)
                        {
                            throw new Exception("Failed to process the file");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.WriteLine($"Error while processing '{file}': {ex.Message}");
                        if (!opts.ContinueOnError)
                        {
                            throw ex;
                        }
                    }

                    m_ProgressHandler?.Report(++curJob / (double)totalJobs);
                }
            }

            m_Logger.WriteLine($"Exporting completed in {DateTime.Now.Subtract(curTime).ToString(@"hh\:mm\:ss")}");
        }

        private Task<bool> StartWaitProcessAsync(ProcessStartInfo prcStartInfo,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var process = new Process();

            var isCancelled = false;

            process.StartInfo = prcStartInfo;
            process.EnableRaisingEvents = true;
            prcStartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, e) =>
            {
                var tag = StandAloneExporter.Program.LOG_MESSAGE_TAG;
                if (e.Data?.StartsWith(tag) == true)
                {
                    m_Logger.WriteLine(e.Data.Substring(tag.Length));
                }
            };
            process.Exited += (sender, args) =>
            {
                if (!isCancelled)
                {
                    tcs.SetResult(process.ExitCode == 0);
                }
            };

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() =>
                {
                    isCancelled = true;
                    process.Kill();
                    tcs.SetCanceled();
                });
            }

            process.Start();
            process.BeginOutputReadLine();
            return tcs.Task;
        }

        private Dictionary<string, string[]> ParseOptions(ExportOptions opts)
        {
            const string EDRW_FORMAT = ".e";

            var outDir = opts.OutputDirectory;

            if (!string.IsNullOrEmpty(outDir))
            {
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
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
        }
    }
}
