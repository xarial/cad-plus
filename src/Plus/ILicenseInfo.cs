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
        Platinum
    }

    public interface ILicenseInfo
    {
        bool IsRegistered { get; }
        EditionType_e Edition { get; }
        DateTime? TrialExpiryDate { get; }
    }
}
