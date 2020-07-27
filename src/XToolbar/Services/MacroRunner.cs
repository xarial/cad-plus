using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad;
using Xarial.XCad.Enums;

namespace Xarial.CadPlus.XToolbar.Services
{
    public interface IMacroRunner
    {
        void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun);
    }

    public class MacroRunner : IMacroRunner
    {
        private readonly IXApplication m_App;

        public MacroRunner(IXApplication app)
        {
            m_App = app;
        }

        public void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun)
        {
            var opts = unloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

            var macro = m_App.OpenMacro(macroPath);
            macro.Run(new XCad.Structures.MacroEntryPoint(entryPoint.ModuleName, entryPoint.SubName), opts);
        }
    }
}