using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Batch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class XCadMacroAttribute : ExportAttribute
    {
        public XCadMacroAttribute() : base(typeof(IXCadMacro)) 
        {
        }
    }
}
