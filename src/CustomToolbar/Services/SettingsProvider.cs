//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.CustomToolbar.Exceptions;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.CustomToolbar.Services
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
            var settsFilePath = Path.Combine(Locations.AppDirectoryPath, Settings.Default.XToolbarSettingsFile);

            if (File.Exists(settsFilePath)) 
            {
                try
                {
                    setts = m_UserSettsSrv.ReadSettings<ToolbarSettings>(settsFilePath);
                }
                catch (Exception ex)
                {
                    throw new UserException($"Failed to read settings file at {settsFilePath}. Remove file to clear settings", ex);
                }
            }
            else
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
            m_UserSettsSrv.StoreSettings(setts, 
                Path.Combine(Locations.AppDirectoryPath, Settings.Default.XToolbarSettingsFile));
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