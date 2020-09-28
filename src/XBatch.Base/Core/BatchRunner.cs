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
            internal List<string> Files { get; set; }
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
                Files = allFiles,
                CurJob = 0,
                TotalJobs = allFiles.Count
            };

            for (int i = 0; i < jobs.Length; i++) 
            {
                jobs[i] = ProcessFiles(progressData, opts, i, token);
            }

            await Task.WhenAll(jobs);

            m_Logger.WriteLine($"Exporting completed in {DateTime.Now.Subtract(curTime).ToString(@"hh\:mm\:ss")}");
        }

        private List<string> SelectAllFiles(IEnumerable<string> inputs, string filter) 
        {
            var files = new List<string>();

            foreach (var input in inputs)
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

            return files;
        }

        private async Task ProcessFiles(BatchRunnerProgressData data, BatchRunnerOptions opts, int jobId,
            CancellationToken cancellationToken = default)
        {
            IXApplication app = null;

            CancellationTokenSource tcs = null;

            void ResetTimeout()
            {
                if (tcs != null)
                {
                    if (opts.Timeout > 0)
                    {
                        tcs.CancelAfter(TimeSpan.FromSeconds(opts.Timeout));
                    }
                }
            }

            if (cancellationToken != default)
            {
                tcs = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                ResetTimeout();

                tcs.Token.Register(() =>
                {
                    TryShutDownApplication(app);
                });
            }

            while (true)
            {
                string nextFile = "";

                try
                {
                    if (app == null)
                    {
                        try
                        {
                            //TODO: get version from options
                            app = await m_AppProvider.StartApplicationAsync(null);
                        }
                        catch (Exception ex)
                        {
                            throw new UserMessageException("Failed to start application", ex);
                        }
                    }

                    lock (m_LockObj)
                    {
                        if (data.Files.Any())
                        {
                            nextFile = data.Files.First();
                            data.Files.RemoveAt(0);
                        }
                        else
                        {
                            break;
                        }
                    }

                    ResetTimeout();

                    if (tcs.IsCancellationRequested)
                    {
                        TryShutDownApplication(app);
                        return;
                    }

                    IXDocument doc = null;

                    try
                    {
                        doc = app.Documents.Open(new DocumentOpenArgs()
                        {
                            Path = nextFile
                        });

                        foreach (var macroPath in opts.Macros)
                        {
                            ResetTimeout();

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

                    lock (m_LockObj)
                    {
                        data.CurJob = data.CurJob + 1;
                        m_ProgressHandler?.Report(data.CurJob / (double)data.TotalJobs);
                    }
                }
                catch (Exception ex)
                {
                    var userErr = ex.ParseUserError(out _);
                    //TODO: add trace logger

                    m_Logger.WriteLine($"Error while processing: {nextFile}: {userErr}");

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
            }

            TryShutDownApplication(app);
        }

        private static void TryCloseDocument(IXDocument doc)
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
