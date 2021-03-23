//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit;

namespace Xarial.CadPlus.Batch.Sw
{
    public class SwApplicationProvider : ICadApplicationInstanceProvider
    {
        private readonly Dictionary<Process, List<string>> m_ForceDisabledAddIns;

        private readonly IXLogger m_Logger;

        private readonly IXServiceCollection m_CustomServices;

        public IMacroExecutor MacroRunnerService { get; }
        public ICadDescriptor EntityDescriptor { get; }

        public SwApplicationProvider(IXLogger logger, IMacroExecutor svc, ICadDescriptor entDesc)
        {
            m_Logger = logger;

            MacroRunnerService = svc;
            EntityDescriptor = entDesc;
            m_CustomServices = new ServiceCollection();
            m_CustomServices.AddOrReplace<IXLogger>(() => m_Logger);

            m_ForceDisabledAddIns = new Dictionary<Process, List<string>>();
        }

        public IEnumerable<IXVersion> GetInstalledVersions()
            => SwApplicationFactory.GetInstalledVersions();

        public IXVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                var installedVers = SwApplicationFactory.GetInstalledVersions();
                if (installedVers.Any())
                {
                    return installedVers.OrderBy(v => (int)v.Major).First();
                }
                else
                {
                    throw new Exception("Failed to find installed version of the host application");
                }
            }
            else if (int.TryParse(version, out int rev))
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), $"Sw{rev}");
                return SwApplicationFactory.CreateVersion(swVers);
            }
            else if (version.StartsWith("solidworks", StringComparison.CurrentCultureIgnoreCase))
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), $"Sw{version.Substring("solidworks".Length).Trim()}");
                return SwApplicationFactory.CreateVersion(swVers);
            }
            else
            {
                var swVers = (SwVersion_e)Enum.Parse(typeof(SwVersion_e), version);
                return SwApplicationFactory.CreateVersion(swVers);
            }
        }

        public IXApplication StartApplication(IXVersion vers, StartupOptions_e opts,
            Action<Process> startingHandler, CancellationToken cancellationToken)
        {
            var app = SwApplicationFactory.PreCreate();
            app.Starting += (s, p) => startingHandler.Invoke(p);

            app.State = ApplicationState_e.Default;
            app.Version = (ISwVersion)vers;

            app.CustomServices = m_CustomServices;

            List<string> forceDisabledAddIns = null;

            if (opts.HasFlag(StartupOptions_e.Safe))
            {
                app.State |= ApplicationState_e.Safe;
                TryDisableAddIns(out forceDisabledAddIns);
            }

            if (opts.HasFlag(StartupOptions_e.Background))
            {
                app.State |= ApplicationState_e.Background;
            }

            if (opts.HasFlag(StartupOptions_e.Silent))
            {
                app.State |= ApplicationState_e.Silent;
            }

            if (opts.HasFlag(StartupOptions_e.Hidden))
            {
                app.State |= ApplicationState_e.Hidden;
            }

            try
            {
                app.Commit(cancellationToken);
            }
            finally
            {
                if (forceDisabledAddIns != null)
                {
                    TryEnableAddIns(forceDisabledAddIns);
                }
            }

            app.Sw.UserControlBackground = true;
            app.Sw.CommandInProgress = true;

            //Note. By some reasons the process from IXApplication::Process does not fire exited event
            var prc = Process.GetProcessById(app.Process.Id);
            prc.EnableRaisingEvents = true;
            prc.Exited += OnProcessExited;
            m_ForceDisabledAddIns.Add(prc, forceDisabledAddIns);

            return app;
        }

        private void App_Starting(IXApplication sender, Process process)
        {
            throw new NotImplementedException();
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            var prc = sender as Process;
            prc.Exited -= OnProcessExited;

            if (m_ForceDisabledAddIns.TryGetValue(prc, out List<string> guids))
            {
                TryEnableAddIns(guids);
            }
            else
            {
                Debug.Assert(false, "Process is not registered or removed");
            }
        }

        private void TryEnableAddIns(List<string> guids)
        {
            try
            {
                if (guids?.Any() == true)
                {
                    SwApplicationFactory.EnableAddInsStartup(guids);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }

        private void TryDisableAddIns(out List<string> guids)
        {
            try
            {
                SwApplicationFactory.DisableAllAddInsStartup(out guids);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                guids = null;
            }
        }

        public bool CanProcessFile(string filePath)
        {
            const string TEMP_SW_FILE_NAME = "~$";

            var fileName = Path.GetFileName(filePath);

            return !fileName.StartsWith(TEMP_SW_FILE_NAME);
        }

        public void Dispose()
        {
            var guids = m_ForceDisabledAddIns.SelectMany(x => x.Value).Distinct();

            TryEnableAddIns(guids.ToList());
        }

        public string GetVersionId(IXVersion value) => (value as ISwVersion)?.Major.ToString();
    }
}
