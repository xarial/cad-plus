//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class VersionVM
    {
        public IXVersion Version { get; }

        public VersionVM(IXVersion vers) 
        {
            Version = vers;
        }

        public override string ToString()
            => Version.DisplayName;

        public override bool Equals(object obj)
        {
            if (obj is VersionVM)
            {
                return (obj as VersionVM).Version.CompareTo(Version) == 0;
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
