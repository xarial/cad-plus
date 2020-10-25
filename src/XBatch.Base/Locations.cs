using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Properties;

namespace Xarial.CadPlus.XBatch.Base
{
    internal static class Locations
    {
        internal static string AppDirectoryPath
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
