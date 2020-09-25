using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.XBatch.Base
{
    public class AppVersionInfo
    {
        public string DisplayName { get; }

        public AppVersionInfo(string dispName) 
        {
            DisplayName = dispName;
        }
    }
}
