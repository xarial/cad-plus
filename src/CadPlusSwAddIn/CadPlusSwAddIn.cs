using System;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.SwAddIn
{
    [ComVisible(true), Guid("6C1F130E-65C3-4237-94B7-A26B3B3AD282")]
    public class CadPlusSwAddIn : SwAddInEx
    {
        public override void OnConnect()
        {
        }
    }
}
