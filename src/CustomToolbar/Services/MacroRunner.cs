//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Enums;
using Xarial.XCad.Structures;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IMacroRunner
    {
        bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo);
    }

    public class MacroRunner : IMacroRunner
    {
        private readonly IXApplication m_App;
        private readonly IMacroExecutor m_Runner;

        private readonly IToolbarModuleProxy m_ToolbarModuleProxy;
        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        public MacroRunner(IXApplication app, IMacroExecutor runner, IMessageService msgSvc, IXLogger logger, IToolbarModuleProxy toolbarModuleProxy)
        {
            m_App = app;
            m_Runner = runner;
            m_ToolbarModuleProxy = toolbarModuleProxy;
            m_MsgSvc = msgSvc;
            m_Logger = logger;
        }

        public bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo)
        {
            try
            {
                m_Logger.Log($"Invoking '{trigger}' trigger for '{macroInfo.Title}'", LoggerMessageSeverity_e.Debug);

                TriggerType_e trgType;

                switch (trigger)
                {
                    case Triggers_e.Button:
                        trgType = TriggerType_e.Button;
                        break;
                    case Triggers_e.ToggleButton:
                        trgType = TriggerType_e.ToggleButton;
                        break;
                    case Triggers_e.ApplicationStart:
                        trgType = TriggerType_e.ApplicationStart;
                        break;
                    case Triggers_e.DocumentNew:
                        trgType = TriggerType_e.DocumentNew;
                        break;
                    case Triggers_e.DocumentOpen:
                        trgType = TriggerType_e.DocumentOpen;
                        break;
                    case Triggers_e.DocumentActivated:
                        trgType = TriggerType_e.DocumentActivated;
                        break;
                    case Triggers_e.DocumentSave:
                        trgType = TriggerType_e.DocumentSave;
                        break;
                    case Triggers_e.DocumentClose:
                        trgType = TriggerType_e.DocumentClose;
                        break;
                    case Triggers_e.NewSelection:
                        trgType = TriggerType_e.NewSelection;
                        break;
                    case Triggers_e.ConfigurationChange:
                        trgType = TriggerType_e.ConfigurationChange;
                        break;
                    case Triggers_e.Rebuild:
                        trgType = TriggerType_e.Rebuild;
                        break;
                    default:
                        throw new NotSupportedException($"{trigger} is not supported");
                }

                var trgArgs = new MacroRunArguments()
                {
                    MacroPath = macroInfo.MacroPath,
                    EntryPoint = macroInfo.EntryPoint,
                    UnloadAfterRun = macroInfo.UnloadAfterRun,
                    Arguments = macroInfo.Arguments,
                    Cancel = false
                };

                m_ToolbarModuleProxy.RunMacro(trgType, trgArgs);
                
                if (!trgArgs.Cancel)
                {
                    var opts = trgArgs.UnloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

                    m_Runner.RunMacro(m_App, trgArgs.MacroPath,
                        new MacroEntryPoint(trgArgs.EntryPoint.ModuleName, trgArgs.EntryPoint.SubName), opts, trgArgs.Arguments, null);
                }
                else
                {
                    m_Logger.Log($"Trigger '{trigger}' for '{macroInfo.Title}' invoking cancelled", LoggerMessageSeverity_e.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex, $"Failed to run a macro '{macroInfo.Title}' on trigger '{trigger}'");
                return false;
            }
        }
    }
}