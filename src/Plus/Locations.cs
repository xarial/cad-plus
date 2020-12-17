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
using Xarial.CadPlus.Plus.Properties;

namespace Xarial.CadPlus.Plus
{
    public static class Locations
    {
        public static string AppDirectoryPath
        {
            get
            {
                var appDir = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    Settings.Default.AppRootDir);

                return appDir;
            }
        }
    }
}
