using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;

namespace Xarial.CadPlus.XBatch.Base
{
    public interface IApplicationProvider
    {
        IEnumerable<AppVersionInfo> GetInstalledVersions();
        Task<IXApplication> StartApplicationAsync();
    }
}
