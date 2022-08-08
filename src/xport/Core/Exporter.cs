//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Xport.Core
{
    internal class JobItem : IJobItem
    {
        public event JobItemStatusChangedDelegate StatusChanged;
        public event JobItemIssuesChanged IssuesChanged;

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

        public IReadOnlyList<IJobItemIssue> Issues => m_Issues;

        private JobItemStatus_e m_Status;

        private List<IJobItemIssue> m_Issues;

        internal JobItem(string filePath)
        {
            FilePath = filePath;
            m_Status = JobItemStatus_e.AwaitingProcessing;
            m_Issues = new List<IJobItemIssue>();
        }
    }

    internal class JobItemFile : JobItem//, IJobItemDocument
    {
        internal JobItemFile(string filePath, JobItemFormat[] formats) : base(filePath)
        {
            Formats = formats;
        }

        public JobItemFormat[] Formats { get; }
    }

    internal class JobItemFormat : JobItem//, IJobItemOperation
    {
        internal JobItemFormat(string filePath) : base(filePath)
        {
        }
    }

    public class Exporter : IBatchJobExecutor
    {
        private readonly IJobManager m_JobMgr;

        public event JobSetDelegate JobSet;
        public event JobItemProcessedDelegate ItemProcessed;
        public event JobStatusChangedDelegate StatusChanged;
        public event JobLogDelegateDelegate Log;
        public event JobCompletedDelegate JobCompleted;

        private readonly ExportOptions m_Opts;

        public Exporter(IJobManager jobMgr, ExportOptions opts)
        {
            m_JobMgr = jobMgr;
            m_Opts = opts;
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
                    Log?.Invoke(this, e.Data.Substring(tag.Length));
                }
            };

            process.Exited += (sender, args) =>
            {
                if (!isCancelled)
                {
                    tcs.SetResult(process.ExitCode == 0);
                }
            };

            if (cancellationToken != default)
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
            m_JobMgr.AddProcess(process);
            process.BeginOutputReadLine();
            return tcs.Task;
        }

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            Log?.Invoke(this, $"Exporting Started");

            var startTime = DateTime.Now;

            var jobs = ParseOptions(m_Opts);

            var formats = jobs.SelectMany(j => j.Formats).ToArray();

            JobSet?.Invoke(this, formats, startTime);

            var jobItemIndex = 0;

            foreach (var job in jobs)
            {
                var file = job.FilePath;

                var outFiles = job.Formats;

                foreach (var outFile in outFiles)
                {
                    try
                    {
                        var desFile = outFile.FilePath;

                        StatusChanged?.Invoke(this, $"Exporting '{file}' to '{desFile}'");

                        int index = 0;

                        while (File.Exists(desFile))
                        {
                            var outDir = Path.GetDirectoryName(outFile.FilePath);
                            var fileName = Path.GetFileNameWithoutExtension(outFile.FilePath);
                            var ext = Path.GetExtension(outFile.FilePath);

                            fileName = $"{fileName} ({++index})";

                            desFile = Path.Combine(outDir, fileName + ext);
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            Log?.Invoke(this, $"Cancelled by the user");
                            return false;
                        }

                        var prcStartInfo = new ProcessStartInfo()
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            FileName = typeof(StandAloneExporter.Program).Assembly.Location,
                            Arguments = $"\"{file}\" \"{desFile}\" {m_Opts.Version}"
                        };

                        var tcs = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        if (m_Opts.Timeout > 0)
                        {
                            tcs.CancelAfter(TimeSpan.FromSeconds(m_Opts.Timeout));
                        }

                        var res = await StartWaitProcessAsync(prcStartInfo, tcs.Token).ConfigureAwait(false);

                        if (res)
                        {
                            outFile.Status = JobItemStatus_e.Succeeded;
                        }
                        else
                        {
                            throw new Exception("Failed to process the file");
                        }
                    }
                    catch (Exception ex)
                    {
                        outFile.Status = JobItemStatus_e.Failed;

                        Log?.Invoke(this, $"Error while processing '{file}': {ex.Message}");
                        if (!m_Opts.ContinueOnError)
                        {
                            throw ex;
                        }
                    }

                    jobItemIndex++;
                    ItemProcessed?.Invoke(this, outFile, (double)jobItemIndex / (double)formats.Length, true);
                }

                if (outFiles.All(f => f.Status == JobItemStatus_e.Succeeded))
                {
                    job.Status = JobItemStatus_e.Succeeded;
                }
                else if (outFiles.All(f => f.Status == JobItemStatus_e.Failed))
                {
                    job.Status = JobItemStatus_e.Failed;
                }
                else
                {
                    job.Status = JobItemStatus_e.Warning;
                }
            }

            var duration = DateTime.Now.Subtract(startTime);
            
            Log?.Invoke(this, $"Exporting completed in {duration.ToString(@"hh\:mm\:ss")}");
            
            JobCompleted?.Invoke(this, duration);

            return true;
        }

        public bool Execute(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken).Result;

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