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
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Structures;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IMacroRunner
    {
        bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo, IXDocument targetDoc);
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

        public bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo, IXDocument targetDoc)
        {
            try
            {
                m_Logger.Log($"Invoking '{trigger}' trigger for '{macroInfo.Title}'", LoggerMessageSeverity_e.Debug);

                EventType_e eventType;

                switch (trigger)
                {
                    case Triggers_e.Button:
                        eventType = EventType_e.ButtonClick;
                        break;
                    case Triggers_e.ToggleButton:
                        eventType = EventType_e.ToggleButtonCheck;
                        break;
                    case Triggers_e.ApplicationStart:
                        eventType = EventType_e.ApplicationStart;
                        break;
                    case Triggers_e.DocumentNew:
                        eventType = EventType_e.DocumentNew;
                        break;
                    case Triggers_e.DocumentOpen:
                        eventType = EventType_e.DocumentOpen;
                        break;
                    case Triggers_e.DocumentActivated:
                        eventType = EventType_e.DocumentActivated;
                        break;
                    case Triggers_e.DocumentSave:
                        eventType = EventType_e.DocumentSave;
                        break;
                    case Triggers_e.DocumentClose:
                        eventType = EventType_e.DocumentClose;
                        break;
                    case Triggers_e.NewSelection:
                        eventType = EventType_e.NewSelection;
                        break;
                    case Triggers_e.ConfigurationChange:
                        eventType = EventType_e.ConfigurationChange;
                        break;
                    case Triggers_e.Rebuild:
                        eventType = EventType_e.Rebuild;
                        break;
                    default:
                        throw new NotSupportedException($"{trigger} is not supported");
                }

                var eventArgs = new MacroRunningArguments(macroInfo,  targetDoc)
                {
                    Cancel = false
                };

                m_ToolbarModuleProxy.CallMacroRunning(eventType, eventArgs);
                
                if (!eventArgs.Cancel)
                {
                    var opts = eventArgs.MacroInfo.UnloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

                    m_Runner.RunMacro(m_App, eventArgs.MacroInfo.MacroPath,
                        new MacroEntryPoint(eventArgs.MacroInfo.EntryPoint.ModuleName, eventArgs.MacroInfo.EntryPoint.SubName),
                        opts, eventArgs.MacroInfo.Arguments, null);

                    return true;
                }
                else
                {
                    m_Logger.Log($"Trigger '{trigger}' for '{macroInfo.Title}' invoking cancelled", LoggerMessageSeverity_e.Debug);
                    return false;
                }
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