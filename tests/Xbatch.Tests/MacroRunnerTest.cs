using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.MacroRunner;
using Xarial.XCad.SolidWorks;

namespace Xbatch.Tests
{
    public class MacroRunnerTest
    {
        [Test]
        public void RunMacroWithParameters() 
        {
            var app = SwApplicationFactory.Create();

            //app = SwApplicationFactory.FromProcess(Process.GetProcessById(26320));
            
            var runner = (IMacroRunner)Activator.CreateInstance(Type.GetTypeFromProgID("CadPlus.MacroRunner.Sw"));

            var res = (IStatusResult)runner.Run(app.Sw, "D:\\Temp\\ParamsMacro.swp", "ParamsMacro1", "main", 0, new ArgumentsParameter(new string[] { "A", "B" }), true);
        }
    }
}
