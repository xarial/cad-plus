//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchJobVersionTransformer : BaseUserSettingsVersionsTransformer
    {
    }

    [UserSettingVersion("1.0.0", typeof(BatchJobVersionTransformer))]
    public class BatchJob
    {
        public string[] Input { get; set; }
        public string[] Filters { get; set; }
        
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
        public string[] Macros { get; set; }

        public AppVersionInfo Version { get; set; }
        public StartupOptions_e StartupOptions { get; set; }
        public OpenFileOptions_e OpenFileOptions { get; set; }

        public BatchJob() 
        {
            Filters = new string[] { "*.*" };
            Timeout = 600;
            ContinueOnError = true;
            StartupOptions = StartupOptions_e.Silent | StartupOptions_e.Safe;
            OpenFileOptions = OpenFileOptions_e.Silent;
        }
    }
}
