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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Xport.Core;
using Xarial.XToolkit;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Xport.Services
{
    public class JobItemFile : IBatchJobItem
    {
        public event BatchJobItemNestedItemsInitializedDelegate NestedItemsInitialized;

        IReadOnlyList<IBatchJobItemOperation> IBatchJobItem.Operations => Operations;
        IBatchJobItemState IBatchJobItem.State => State;

        public string FilePath { get; }

        internal JobItemFile(string filePath, JobItemExportFormat[] formats)
        {
            FilePath = filePath;
            Operations = formats;
            Title = Path.GetFileName(filePath);
            Description = filePath;
            Link = TryOpenInFileExplorer;
            State = new JobItemState();
        }

        public BitmapImage Icon { get; }
        public BitmapImage Preview { get; }
        public string Title { get; }
        public string Description { get; }

        public Action Link { get; }

        public JobItemState State { get; }

        public IReadOnlyList<JobItemExportFormat> Operations { get; }

        public IReadOnlyList<IBatchJobItem> Nested { get; }

        private void TryOpenInFileExplorer()
        {
            try
            {
                FileSystemUtils.BrowseFileInExplorer(FilePath);
            }
            catch
            {
            }
        }
    }

    public class JobItemExportFormatDefinition : IBatchJobItemOperationDefinition
    {
        public string Name { get; }
        public BitmapImage Icon { get; }

        public string Extension { get; }

        public JobItemExportFormatDefinition(string ext)
        {
            Extension = ext;

            Name = ext;
        }
    }

    public class JobItemExportFormat : IBatchJobItemOperation
    {
        public event BatchJobItemOperationUserResultChangedDelegate UserResultChanged;

        IBatchJobItemState IBatchJobItemOperation.State => State;

        public string OutputFilePath { get; }
        public IBatchJobItemOperationDefinition Definition { get; }

        public JobItemState State { get; }
        public object UserResult { get; }

        public JobItemExportFormat(string outFilePath, JobItemExportFormatDefinition def)
        {
            OutputFilePath = outFilePath;
            Definition = def;
            State = new JobItemState();
        }
    }

    public class Exporter : IAsyncBatchJob
    {
        private readonly IJobProcessManager m_JobMgr;

        public event BatchJobStartedDelegate Started;
        public event BatchJobInitializedDelegate Initialized;
        public event BatchJobItemProcessedDelegate ItemProcessed;
        public event BatchJobLogDelegateDelegate Log;
        public event BatchJobCompletedDelegate Completed;

        private readonly ExportOptions m_Opts;

        public IReadOnlyList<IBatchJobItem> JobItems => m_JobFiles;
        public IReadOnlyList<IBatchJobItemOperationDefinition> OperationDefinitions { get; private set; }
        public IReadOnlyList<string> LogEntries => m_LogEntries;

        public IBatchJobState State => m_State;

        private readonly List<string> m_LogEntries;

        private double? m_Progress;

        private JobItemFile[] m_JobFiles;

        private readonly JobState m_State;

        public Exporter(IJobProcessManager jobMgr, ExportOptions opts)
        {
            m_JobMgr = jobMgr;
            m_Opts = opts;
            m_LogEntries = new List<string>();
            m_State = new JobState();
        }

        public async Task TryExecuteAsync(CancellationToken cancellationToken)
            => await this.HandleExecuteAsync(cancellationToken,
                t => Started?.Invoke(this, t),
                t => m_State.StartTime = t,
                InitAsync,
                () => Initialized?.Invoke(this, JobItems, OperationDefinitions),
                DoWorkAsync,
                d => Completed?.Invoke(this, d, m_State.Status),
                d => m_State.Duration = d,
                s => m_State.Status = s);

        private Task InitAsync(CancellationToken arg)
        {
            AddLogEntry($"Exporting Started");

            m_JobFiles = ParseOptions(m_Opts, out var formats);

            OperationDefinitions = formats;

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < m_JobFiles.Length; i++)
            {
                var file = m_JobFiles[i];

                var outFiles = file.Operations;

                foreach (var outFile in outFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var desFile = outFile.OutputFilePath;

                        int index = 0;

                        while (File.Exists(desFile))
                        {
                            var outDir = Path.GetDirectoryName(outFile.OutputFilePath);
                            var fileName = Path.GetFileNameWithoutExtension(outFile.OutputFilePath);
                            var ext = Path.GetExtension(outFile.OutputFilePath);

                            fileName = $"{fileName} ({++index})";

                            desFile = Path.Combine(outDir, fileName + ext);
                        }

                        var prcStartInfo = new ProcessStartInfo()
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            FileName = typeof(StandAloneExporter.Program).Assembly.Location,
                            Arguments = $"\"{file.FilePath}\" \"{desFile}\" {m_Opts.Version}"
                        };

                        var tcs = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        if (m_Opts.Timeout > 0)
                        {
                            tcs.CancelAfter(TimeSpan.FromSeconds(m_Opts.Timeout));
                        }

                        var res = await StartWaitProcessAsync(prcStartInfo, tcs.Token).ConfigureAwait(false);

                        if (res)
                        {
                            outFile.State.Status = BatchJobItemStateStatus_e.Succeeded;
                        }
                        else
                        {
                            throw new Exception("Failed to process the file");
                        }
                    }
                    catch (Exception ex)
                    {
                        outFile.State.Status = BatchJobItemStateStatus_e.Failed;

                        AddLogEntry($"Error while processing '{file.FilePath}': {ex.Message}");
                        if (!m_Opts.ContinueOnError)
                        {
                            throw ex;
                        }
                    }
                }

                file.State.Status = file.ComposeStatus();

                ItemProcessed?.Invoke(this, file);
                m_State.Progress = (i + 1) / (double)m_JobFiles.Length;
            }
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
                    AddLogEntry(e.Data.Substring(tag.Length));
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

        private JobItemFile[] ParseOptions(ExportOptions opts, out JobItemExportFormatDefinition[] formatDefs)
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

            formatDefs = new JobItemExportFormatDefinition[opts.Format.Length];

            for (int i = 0; i < opts.Format.Length; i++)
            {
                var ext = opts.Format[i];

                if (!ext.StartsWith("."))
                {
                    ext = "." + ext;
                }

                formatDefs[i] = new JobItemExportFormatDefinition(ext);
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
                var outFiles = new JobItemExportFormat[opts.Format.Length];
                jobs.Add(new JobItemFile(file, outFiles));

                for (int i = 0; i < formatDefs.Length; i++)
                {
                    var formatDef = formatDefs[i];

                    var ext = formatDef.Extension;

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

                    outFiles[i] = new JobItemExportFormat(Path.Combine(!string.IsNullOrEmpty(outDir) ? outDir : Path.GetDirectoryName(file),
                        Path.GetFileNameWithoutExtension(file) + ext), formatDef);
                }
            }

            return jobs.ToArray();
        }

        private void AddLogEntry(string msg)
        {
            m_LogEntries.Add(msg);
            Log?.Invoke(this, msg);
        }

        public void Dispose()
        {
        }
    }
}