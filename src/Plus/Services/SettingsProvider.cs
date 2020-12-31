//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad.Reflection;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services.UserSettings;

namespace Xarial.CadPlus.Plus.Services
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly UserSettingsService m_UserSettsSrv;

        public SettingsProvider()
        {
            m_UserSettsSrv = new UserSettingsService();
        }

        public T ReadSettings<T>() where T : new()
        {
            var settsFilePath = GetSettingsFilePath<T>(out string[] altSettsFilePaths);

            if (File.Exists(settsFilePath))
            {
                try
                {
                    return m_UserSettsSrv.ReadSettings<T>(settsFilePath);
                }
                catch (Exception ex)
                {
                    throw new UserException($"Failed to read settings file at {settsFilePath}. Remove file to clear settings", ex);
                }
            }
            else if (altSettsFilePaths?.Any() == true)
            {
                foreach (var altFilePath in altSettsFilePaths)
                {
                    try
                    {
                        return m_UserSettsSrv.ReadSettings<T>(altFilePath);
                    }
                    catch
                    {
                    }
                }

                throw new UserException($"Failed to read settings file from alternative locations: {string.Join(", ", altSettsFilePaths)}. Remove file to clear settings");
            }
            else
            {
                return new T();
            }
        }

        public void WriteSettings<T>(T setts) where T : new()
        {
            var settsFilePath = GetSettingsFilePath<T>(out _);

            var dir = Path.GetDirectoryName(settsFilePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            m_UserSettsSrv.StoreSettings(setts, settsFilePath);
        }

        private string GetSettingsFilePath<T>(out string[] altFilePaths)
        {
            string GetFilePath(string fileName) => Path.Combine(Locations.AppDirectoryPath, fileName);

            string settsFileName;
            altFilePaths = null;

            if (typeof(T).TryGetAttribute(out SettingsAttribute att, true))
            {
                settsFileName = att.SettingsFileName;

                if (att.AltSettsFileNames != null)
                {
                    altFilePaths = att.AltSettsFileNames.Select(f => GetFilePath(f)).ToArray();
                }
            }
            else
            {
                settsFileName = typeof(T).FullName + ".json";
            }

            return GetFilePath(settsFileName);
        }
    }
}
