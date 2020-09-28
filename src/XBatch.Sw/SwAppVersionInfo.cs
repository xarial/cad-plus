using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.CadPlus.XBatch.Sw
{
    public class SwAppVersionInfo : AppVersionInfo
    {
        internal SwVersion_e Version { get; }

        private static string GetVersionDisplayName(SwVersion_e vers) 
            => $"SOLIDWORKS {vers.ToString().Substring("Sw".Length)}";

        public SwAppVersionInfo(SwVersion_e vers) : base(GetVersionDisplayName(vers))
        {
            Version = vers;
        }
    }
}
