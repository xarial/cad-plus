//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.XCad;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XBatch.Base
{
    public interface IApplicationProvider
    {
        IEnumerable<AppVersionInfo> GetInstalledVersions();
        IXApplication StartApplication(AppVersionInfo vers, StartupOptions_e opts, CancellationToken cancellationToken);
        AppVersionInfo ParseVersion(string version);
        bool CanProcessFile(string filePath);
        FileFilter[] InputFilesFilter { get; }
        FileFilter[] MacroFilesFilter { get; }
    }
}
