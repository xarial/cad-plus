//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Services.UserSettings.Attributes;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class BatchJobVersionTransformer : BaseUserSettingsVersionsTransformer
    {
        public BatchJobVersionTransformer()
        {
            Add(new Version("1.0.0"), new Version("1.1.0"), t =>
            {
                var macrosField = t.Children<JProperty>().First(p => p.Name == "Macros");

                var macrosOld = macrosField?.Value as JArray;

                if (macrosOld != null) 
                {
                    var macros = new JArray();

                    foreach (var macro in macrosOld) 
                    {
                        var macroData = new JObject();
                        macroData.Add(new JProperty("FilePath", macro.Value<string>()));
                        macroData.Add(new JProperty("Arguments", null));

                        macros.Add(macroData);
                    }

                    macrosField.Value = macros;
                }

                var versionField = t.Children<JProperty>().First(p => p.Name == "Version");
                var versId = versionField.Value["Id"].ToString();

                versionField.Replace(new JProperty("VersionId", versId));

                return t;
            });

            Add(new Version("1.1.0"), new Version("1.2.0"), t =>
            {
                (t as JObject).Add("ApplicationId", "DsSolidWorks");

                return t;
            });
        }
    }

    [UserSettingVersion("1.2.0", typeof(BatchJobVersionTransformer))]
    public class BatchJob
    {
        internal static BatchJob FromFile(string filePath) 
        {
            var svc = new UserSettingsService();

            var batchJob = svc.ReadSettings<BatchJob>(filePath);

            return batchJob;
        }

        public string ApplicationId { get; set; }

        public string[] Input { get; set; }
        public string[] Filters { get; set; }
        
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
        public MacroData[] Macros { get; set; }

        public string VersionId { get; set; }
        public StartupOptions_e StartupOptions { get; set; }
        public OpenFileOptions_e OpenFileOptions { get; set; }
        public Actions_e Actions { get; set; }
        public int BatchSize { get; set; }

        public BatchJob() 
        {
            Filters = new string[] { "*.*" };
            Timeout = 600;
            BatchSize = 25;
            ContinueOnError = true;
            StartupOptions = StartupOptions_e.Silent | StartupOptions_e.Safe;
            OpenFileOptions = OpenFileOptions_e.Silent | OpenFileOptions_e.ForbidUpgrade;
            Actions = Actions_e.None;
        }
    }

    public static class BatchJobExtension 
    {
        public static IApplicationProvider FindApplicationProvider(this BatchJob job, IApplicationProvider[] appProviders)
        {
            var appProvider = appProviders.FirstOrDefault(
                p => string.Equals(p.ApplicationId, job.ApplicationId,
                StringComparison.CurrentCultureIgnoreCase));

            if (appProvider == null)
            {
                throw new UserException("Failed to find the application provider for this job file");
            }

            return appProvider;
        }
    }
}
