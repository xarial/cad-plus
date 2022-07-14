using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Graphics;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.CadPlus.AddIn.Sw.Services
{
    public class CadPlusTriadHandlerProvider : ITriadHandlerProvider
    {
        public SwTriadHandler CreateHandler(ISwApplication app) => new SwGeneralTriadHandler();
    }
}
