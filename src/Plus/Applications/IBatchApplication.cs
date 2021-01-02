using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Plus.Applications
{
    [Flags]
    public enum StartupOptions_e
    {
        Default = 0,

        [Summary("Bypasses all settings")]
        Safe = 1,

        [Summary("Runs host application in background")]
        Background = 2,

        [Summary("Suppresses all popup windows")]
        Silent = 4,

        [Summary("Hides the main window of the application")]
        Hidden = 8
    }

    public interface IApplicationProvider : IDisposable
    {
        string DisplayName { get; }
        string ApplicationId { get; }
        Image ApplicationIcon { get; }

        IMacroFileFilterProvider MacroFileFiltersProvider { get; }
        IMacroRunnerExService MacroRunnerService { get; }

        IEnumerable<IXVersion> GetInstalledVersions();
        IXApplication StartApplication(IXVersion vers, StartupOptions_e opts,
            CancellationToken cancellationToken);
        IXVersion ParseVersion(string version);
        bool CanProcessFile(string filePath);
        FileTypeFilter[] InputFilesFilter { get; }
        string GetVersionId(IXVersion value);
    }

    public interface IBatchApplication : IApplication
    {
        IApplicationProvider[] ApplicationProviders { get; }
        void RegisterApplicationProvider(IApplicationProvider provider);
    }
}
