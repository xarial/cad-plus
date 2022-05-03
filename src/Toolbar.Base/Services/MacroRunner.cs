//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Toolbar.Services;
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
        bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo, IXDocument targetDoc, string workDir);
    }

    public class MacroRunner : IMacroRunner
    {
        private readonly IXApplication m_App;
        private readonly IMacroExecutor m_Runner;

        private readonly IToolbarModuleProxy m_ToolbarModuleProxy;
        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;
        private readonly IFilePathResolver m_FilePathResolver;

        public MacroRunner(IXApplication app, IMacroExecutor runner, IMessageService msgSvc, IXLogger logger,
            IToolbarModuleProxy toolbarModuleProxy, IFilePathResolver filePathResolver)
        {
            m_App = app;
            m_Runner = runner;
            m_ToolbarModuleProxy = toolbarModuleProxy;
            m_MsgSvc = msgSvc;
            m_Logger = logger;
            m_FilePathResolver = filePathResolver;
        }

        public bool TryRunMacroCommand(Triggers_e trigger, CommandMacroInfo macroInfo, IXDocument targetDoc, string workDir)
        {
            try
            {
                m_Logger.Log($"Invoking '{trigger}' trigger for '{macroInfo.Title}'", LoggerMessageSeverity_e.Debug);
                
                var eventType = GetEventType(trigger);

                var eventArgs = new MacroRunningArguments(macroInfo, targetDoc)
                {
                    Cancel = false
                };

                m_ToolbarModuleProxy.CallMacroRunning(eventType, eventArgs);

                if (!eventArgs.Cancel)
                {
                    var opts = eventArgs.MacroInfo.UnloadAfterRun ? MacroRunOptions_e.UnloadAfterRun : MacroRunOptions_e.Default;

                    var macroPath = m_FilePathResolver.Resolve(eventArgs.MacroInfo.MacroPath, workDir);

                    m_Logger.Log($"Running macro '{macroPath}' with arguments '{eventArgs.MacroInfo.Arguments}'", LoggerMessageSeverity_e.Debug);

                    var entryPoint = eventArgs.MacroInfo.EntryPoint;

                    if (entryPoint == null)
                    {
                        throw new UserException($"Entry point is not specified for macro '{macroInfo.Title}'");
                    }

                    m_Runner.RunMacro(m_App, macroPath,
                        new MacroEntryPoint(entryPoint.ModuleName, entryPoint.SubName),
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

        private static EventType_e GetEventType(Triggers_e trigger)
        {
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
                case Triggers_e.Selection:
                    eventType = EventType_e.NewSelection;
                    break;
                case Triggers_e.ConfigurationSheetChange:
                    eventType = EventType_e.ConfigurationChange;
                    break;
                case Triggers_e.Rebuild:
                    eventType = EventType_e.Rebuild;
                    break;
                default:
                    throw new NotSupportedException($"{trigger} is not supported");
            }

            return eventType;
        }
    }
}