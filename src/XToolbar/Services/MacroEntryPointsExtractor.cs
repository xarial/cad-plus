using System.Linq;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad;

namespace Xarial.CadPlus.XToolbar.Services
{
    public interface IMacroEntryPointsExtractor
    {
        MacroStartFunction[] GetEntryPoints(string macroPath);
    }

    public class MacroEntryPointsExtractor : IMacroEntryPointsExtractor
    {
        private readonly IXApplication m_App;

        public MacroEntryPointsExtractor(IXApplication app)
        {
            m_App = app;
        }

        public MacroStartFunction[] GetEntryPoints(string macroPath)
        {
            return m_App.OpenMacro(macroPath).EntryPoints.Select(x => new MacroStartFunction()
            {
                ModuleName = x.ModuleName,
                SubName = x.ProcedureName
            }).ToArray();
        }
    }
}