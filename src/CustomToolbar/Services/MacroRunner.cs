//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IMacroRunner
    {
        void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun, string args);
    }

    public class MacroRunner : IMacroRunner
    {
        private readonly IXApplication m_App;
        private readonly IMacroRunnerExService m_Runner;

        public MacroRunner(IXApplication app, IMacroRunnerExService runner)
        {
            m_App = app;
            m_Runner = runner;
        }

        public void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun, string args)
        {
            var opts = unloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

            m_Runner.RunMacro(m_App, macroPath,
                new XCad.Structures.MacroEntryPoint(entryPoint.ModuleName, entryPoint.SubName),
                opts, args, null);
        }
    }
}