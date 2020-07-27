//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.IO;
using Xarial.CadPlus.XToolbar.Properties;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.XToolbar.Services
{
    public interface ISettingsProvider
    {
        ToolbarSettings GetSettings();

        void SaveSettings(ToolbarSettings setts);
    }

    public class SettingsProvider : ISettingsProvider
    {
        private readonly UserSettingsService m_UserSettsSrv;

        public SettingsProvider(UserSettingsService userSettsSrv)
        {
            m_UserSettsSrv = userSettsSrv;
        }

        public ToolbarSettings GetSettings()
        {
            ToolbarSettings setts;
            try
            {
                setts = m_UserSettsSrv.ReadSettings<ToolbarSettings>(
                    Settings.Default.XToolbarSettingsFile);
            }
            catch
            {
                setts = new ToolbarSettings()
                {
                    SpecificationFile = ToolbarsDefaultSpecFilePath
                };
            }

            return setts;
        }

        public void SaveSettings(ToolbarSettings setts)
        {
            m_UserSettsSrv.StoreSettings(setts, Settings.Default.XToolbarSettingsFile);
        }

        private string ToolbarsDefaultSpecFilePath
        {
            get
            {
                var dataFile = Path.Combine(Locations.AppDirectoryPath,
                    Settings.Default.ToolbarsSpecFile);

                return dataFile;
            }
        }
    }
}