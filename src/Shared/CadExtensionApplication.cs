using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.XCad.Extensions;

namespace Xarial.CadPlus.Plus.Shared
{
    public class CadExtensionApplication : ICadExtensionApplication
    {
        public IXExtension Extension { get; }

        public CadExtensionApplication(IXExtension ext)
        {
            Extension = ext;
        }
    }
}
