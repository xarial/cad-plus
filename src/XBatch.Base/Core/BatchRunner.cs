//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchRunner : IDisposable
    {
        private class BatchRunnerProgressData 
        {
            internal ConcurrentStack<string> Files { get; set; }
            internal int CurJob { get; set; }
            internal int TotalJobs { get; set; }
        }

        private readonly TextWriter m_Logger;
        private readonly IProgress<double> m_ProgressHandler;
        private readonly IApplicationProvider m_AppProvider;

        private readonly object m_LockObj;

        public BatchRunner(IApplicationProvider appProvider, TextWriter logger, IProgress<double> progressHandler)
        {
            m_Logger = logger;
            m_ProgressHandler = progressHandler;
            m_AppProvider = appProvider;
            
            m_LockObj = new object();
        }

        public async Task BatchRun(BatchRunnerOptions opts, CancellationToken token = default)
        {
            m_Logger.WriteLine($"Batch macro running started");

            var curTime = DateTime.Now;
            
            var jobs = new Task[opts.ParallelJobsCount];

            var allFiles = SelectAllFiles(opts.Input, opts.Filter);

            var progressData = new BatchRunnerProgressData()
            {
                Files = new ConcurrentStack<string>(allFiles),
                CurJob = 0
            };

            progressData.TotalJobs = progressData.Files.Count;

            m_Logger.WriteLine($"Running {jobs.Length} parallel job(s) for {progressData.TotalJobs} file(s)");

            for (int i = 0; i < jobs.Length; i++) 
            {
                jobs[i] = ProcessFiles(progressData, opts, i, token);
            }

            await Task.WhenAll(jobs);

            m_Logger.WriteLine($"Batch running completed in {DateTime.Now.Subtract(curTime).ToString(@"hh\:mm\:ss")}");
        }

        private IEnumerable<string> SelectAllFiles(IEnumerable<string> inputs, string filter) 
        {
            foreach (var input in inputs)
            {
                if (Directory.Exists(input))
                {
                    foreach (var file in Directory.EnumerateFiles(input, filter, SearchOption.AllDirectories))
                    {
                        yield return file;
                    }
                }
                else if (File.Exists(input))
                {
                    yield return input;
                }
                else
                {
                    throw new Exception("Specify input file or directory");
                }
            }
        }

        private async Task ProcessFiles(BatchRunnerProgressData data, BatchRunnerOptions opts, int jobId,
            CancellationToken cancellationToken = default)
        {
            IXApplication app = null;

            CancellationTokenSource tcs = null;

            TimeSpan timeout = default;

            if (opts.Timeout > 0) 
            {
                timeout = TimeSpan.FromSeconds(opts.Timeout);
            }

            if (cancellationToken != default)
            {
                tcs = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                ResetTimeout(tcs, timeout);

                tcs.Token.Register(() =>
                {
                    m_Logger.WriteLine("Cancelled by the user");
                    TryShutDownApplication(app);
                });
            }

            while (true)
            {
                string nextDocPath = "";
                var nextMacrosStack = new ConcurrentStack<string>();

                try
                {
                    if (app == null)
                    {
                        try
                        {
                            app = await m_AppProvider.StartApplicationAsync(opts.Version, opts.RunInBackground);
                        }
                        catch (Exception ex)
                        {
                            throw new UserMessageException("Failed to start application", ex);
                        }
                    }

                    if (!data.Files.TryPop(out nextDocPath))
                    {
                        break;
                    }

                    nextMacrosStack = new ConcurrentStack<string>(opts.Macros);

                    ResetTimeout(tcs, timeout);

                    if (tcs.IsCancellationRequested)
                    {
                        TryShutDownApplication(app);
                        return;
                    }

                    m_Logger.WriteLine($"Processing file {nextDocPath} with job: {jobId}");

                    RunMacrosForDocument(app, nextDocPath, nextMacrosStack, tcs, timeout);
                }
                catch (Exception ex)
                {
                    var userErr = ex.ParseUserError(out _);
                    //TODO: add trace logger

                    m_Logger.WriteLine($"Error while processing: {nextDocPath}: {userErr}");

                    if (opts.ContinueOnError)
                    {
                        if (app.Process.HasExited || !app.Process.Responding)
                        {
                            TryShutDownApplication(app);
                            app = null;
                        }
                    }
                    else
                    {
                        TryShutDownApplication(app);
                        throw ex;
                    }
                }
                finally 
                {
                    lock (m_LockObj)
                    {
                        data.CurJob = data.CurJob + 1;
                        m_ProgressHandler?.Report(data.CurJob / (double)data.TotalJobs);
                    }
                }
            }

            TryShutDownApplication(app);
        }

        void ResetTimeout(CancellationTokenSource tcs, TimeSpan timeout)
        {
            if (tcs != null)
            {
                if (timeout != default)
                {
                    tcs.CancelAfter(timeout);
                }
            }
        }

        private void RunMacrosForDocument(IXApplication app, string filePath, ConcurrentStack<string> macros,
            CancellationTokenSource tcs, TimeSpan timeout) 
        {
            IXDocument doc = null;

            try
            {
                doc = app.Documents.Open(new DocumentOpenArgs()
                {
                    Path = filePath
                });

                while (macros.TryPop(out string macroPath)) 
                {
                    ResetTimeout(tcs, timeout);

                    if (tcs.IsCancellationRequested)
                    {
                        TryShutDownApplication(app);
                        return;
                    }

                    try
                    {
                        var macro = app.OpenMacro(macroPath);
                        macro.Run();
                    }
                    catch (MacroRunFailedException ex)
                    {
                        throw new UserMessageException(ex.Message, ex);
                    }
                    catch (OpenDocumentFailedException ex)
                    {
                        throw new UserMessageException(ex.Message, ex);
                    }
                }
            }
            finally
            {
                TryCloseDocument(doc);
            }
        }

        private void TryCloseDocument(IXDocument doc)
        {
            if (doc != null)
            {
                try
                {
                    doc.Close();
                }
                catch
                {
                }
            }
        }

        private void TryShutDownApplication(IXApplication app)
        {
            m_Logger.WriteLine("Closing host application");

            Process appPrc = null;

            try
            {
                if (app != null)
                {
                    appPrc = app.Process;
                    app.Close();
                }
            }
            catch 
            {
            }
            finally
            {
                if (appPrc != null)
                {
                    if (!appPrc.HasExited)
                    {
                        appPrc.Kill();
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
