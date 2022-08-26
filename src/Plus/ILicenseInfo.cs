//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    public enum EditionType_e
    {
        Community = 0,
        Standard = 1,
        Professional = 2,
        Premium = 3,
        Platinum = 4
    }

    public interface ILicenseInfo
    {
        bool IsRegistered { get; }
        EditionType_e Edition { get; }
        DateTime? TrialExpiryDate { get; }
    }

    public interface ILicenseInfoProvider 
    {
        ILicenseInfo ProvideLicense();
    }
}
