using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.AddIn.Base;
using Xarial.CadPlus.Plus.Applications;
using Xarial.XCad.Extensions;

namespace Xarial.CadPlus.AddIn.Sw
{
    public class SwAddInApplication : CadExtensionApplication, ISwAddInApplication
    {
        public SwAddInApplication(IXExtension ext) : base(ext)
        {
        }
    }
}
