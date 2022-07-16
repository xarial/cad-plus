//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Linq;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XCad;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IMacroEntryPointsExtractor
    {
        MacroStartFunction[] GetEntryPoints(string macroPath, string workDir);
    }

    public class MacroEntryPointsExtractor : IMacroEntryPointsExtractor
    {
        private readonly IXApplication m_App;
        private readonly IFilePathResolver m_FilePathResolver;

        public MacroEntryPointsExtractor(IXApplication app, IFilePathResolver filePathResolver)
        {
            m_App = app;
            m_FilePathResolver = filePathResolver;
        }

        public MacroStartFunction[] GetEntryPoints(string macroPath, string workDir)
        {
            var path = m_FilePathResolver.Resolve(macroPath, workDir);

            //TODO: implement check for xCAD macro
            return m_App.OpenMacro(path).EntryPoints.Select(
                x => new MacroStartFunction(x.ModuleName, x.ProcedureName)).ToArray();
        }
    }
}