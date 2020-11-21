//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IMacroRunner
    {
        void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun);
    }

    public class MacroRunner : IMacroRunner
    {
        private readonly IXApplication m_App;

        private readonly IXLogger m_Logger;

        public MacroRunner(IXApplication app, IXLogger logger)
        {
            m_App = app;
            m_Logger = logger;
        }

        public void RunMacro(string macroPath, MacroStartFunction entryPoint, bool unloadAfterRun)
        {
            var opts = unloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

            var macro = m_App.OpenMacro(macroPath);

            try 
            {
                macro.Run(new XCad.Structures.MacroEntryPoint(entryPoint.ModuleName, entryPoint.SubName), opts);
            }
            catch (MacroUserInterruptException userInterruptEx) //do not consider this as an error
            {
                m_Logger.Log(userInterruptEx);
            }
        }
    }
}