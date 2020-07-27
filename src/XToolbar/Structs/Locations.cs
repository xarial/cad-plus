//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.CadPlus.XToolbar.Properties;

namespace Xarial.CadPlus.XToolbar.Structs
{
    internal static class Locations
    {
        internal static string AppDirectoryPath
        {
            get
            {
                var appDir = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                    Settings.Default.AppRootDir);

                return appDir;
            }
        }
    }
}