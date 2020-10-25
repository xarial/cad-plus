//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Xport.Core
{
    internal class JobItem : IJobItem
    {
        public event Action<IJobItem, JobItemStatus_e> StatusChanged;

        public string DisplayName { get; protected set; }

        internal string FilePath { get; }

        public JobItemStatus_e Status
        {
            get => m_Status;
            set
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        private JobItemStatus_e m_Status;

        internal JobItem(string filePath)
        {
            FilePath = filePath;
            m_Status = JobItemStatus_e.AwaitingProcessing;
        }
    }

    internal class JobItemFile : JobItem, IJobItemFile
    {
        public IEnumerable<IJobItemOperation> Operations => throw new NotImplementedException();

        internal JobItemFile(string filePath, JobItemFormat[] formats) : base(filePath)
        {
            Formats = formats;
        }

        public JobItemFormat[] Formats { get; }
    }

    internal class JobItemFormat : JobItem, IJobItemOperation
    {
        internal JobItemFormat(string filePath) : base(filePath)
        {
        }
    }

    public class Exporter : IDisposable
    {
        private readonly TextWriter m_Logger;
        private readonly IProgressHandler m_ProgressHandler;

        public Exporter(TextWriter logger, IProgressHandler progressHandler)
        {
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
        }

        public async Task Export(ExportOptions opts, CancellationToken token = default)
        {
            m_Logger.WriteLine($"Exporting Started");

            var curTime = DateTime.Now;

            var jobs = ParseOptions(opts);
            
            foreach (var job in jobs)
            {
                var file = job.FilePath;

                var outFiles = job.Formats;

                foreach (var outFile in outFiles)
                {
                    try
                    {
                        var desFile = outFile.FilePath;

                        int index = 0;

                        while (File.Exists(desFile))
                        {
                            var outDir = Path.GetDirectoryName(outFile.FilePath);
                            var fileName = Path.GetFileNameWithoutExtension(outFile.FilePath);
                            var ext = Path.GetExtension(outFile.FilePath);

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
                            Arguments = $"\"{file}\" \"{desFile}\""
                        };

                        var tcs = CancellationTokenSource.CreateLinkedTokenSource(token);
                        if (opts.Timeout > 0)
                        {
                            tcs.CancelAfter(TimeSpan.FromSeconds(opts.Timeout));
                        }

                        var res = await StartWaitProcessAsync(prcStartInfo, tcs.Token).ConfigureAwait(false);

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
                }

                m_ProgressHandler?.ReportProgress(job, true);
            }

            m_Logger.WriteLine($"Exporting completed in {DateTime.Now.Subtract(curTime).ToString(@"hh\:mm\:ss")}");
        }

        private Task<bool> StartWaitProcessAsync(ProcessStartInfo prcStartInfo,
            CancellationToken cancellationToken = default)
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
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    tcs.TrySetCanceled();
                });
            }

            process.Start();
            process.BeginOutputReadLine();
            return tcs.Task;
        }

        private JobItemFile[] ParseOptions(ExportOptions opts)
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

            var jobs = new List<JobItemFile>();

            foreach (var file in files)
            {
                var outFiles = new JobItemFormat[opts.Format.Length];
                jobs.Add(new JobItemFile(file, outFiles));

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

                    outFiles[i] = new JobItemFormat(Path.Combine(!string.IsNullOrEmpty(outDir) ? outDir : Path.GetDirectoryName(file),
                        Path.GetFileNameWithoutExtension(file) + ext));
                }
            }

            return jobs.ToArray();
        }

        public void Dispose()
        {
        }
    }
}