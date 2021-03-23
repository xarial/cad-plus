//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Plus.Applications
{
    [Flags]
    public enum StartupOptions_e
    {
        Default = 0,

        [Description("Bypasses all settings")]
        Safe = 1,

        [Description("Runs host application in background")]
        Background = 2,

        [Description("Suppresses all popup windows")]
        Silent = 4,

        [Description("Hides the main window of the application")]
        Hidden = 8
    }

    public interface ICadApplicationInstanceProvider : IDisposable
    {   
        IMacroExecutor MacroRunnerService { get; }
        ICadEntityDescriptor EntityDescriptor { get; }

        IEnumerable<IXVersion> GetInstalledVersions();
        IXApplication StartApplication(IXVersion vers, StartupOptions_e opts,
            Action<Process> startingHandler, CancellationToken cancellationToken);
        IXVersion ParseVersion(string version);
        
        bool CanProcessFile(string filePath);
        string GetVersionId(IXVersion value);
    }

    public class BatchApplicationCommandManager 
    {
        public static class InputTab
        {
            public const string Name = "Input";

            public const string FilesGroupName = "Files";
            public const string FolderFiltersGroupName = "FolderFilters";
            public const string MacrosGroupName = "Macros";
        }

        public static class SettingsTab
        {
            public const string Name = "Settings";

            public const string StartupOptionsGroupName = "StartupOptions";
            public const string FileOpenOptionsGroupName = "FileOpenOptions";
            public const string ProtectionGroupName = "Protection";
            public const string ActionsGroupName = "Actions";
            public const string ResilienceGroupName = "Resilience";
        }

        public static class JobTab 
        {
            public const string Name = "Job";

            public const string ExecutionGroupName = "Execution";
        }
    }

    public interface IBatchApplication : IHasCommandManager
    {
        event ProcessBatchInputDelegate ProcessInput;
        
        ICadApplicationInstanceProvider[] ApplicationProviders { get; }
        void RegisterApplicationProvider(ICadApplicationInstanceProvider provider);
    }
}
