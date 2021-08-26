//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Linq;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad;

namespace Xarial.CadPlus.CustomToolbar.Services
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
            //TODO: implement check for xCAD macro
            return m_App.OpenMacro(macroPath).EntryPoints.Select(
                x => new MacroStartFunction(x.ModuleName, x.ProcedureName)).ToArray();
        }
    }
}