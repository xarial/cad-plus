//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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

        public static bool operator ==(SwAppVersionInfo obj1, SwAppVersionInfo obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }
            
            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(SwAppVersionInfo obj1, SwAppVersionInfo obj2)
        {
            return !(obj1 == obj2);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is SwAppVersionInfo)
            {
                return (obj as SwAppVersionInfo).Version == Version;
            }
            else 
            {
                return false;
            }
        }

        public override int GetHashCode() => 0;
    }
}
