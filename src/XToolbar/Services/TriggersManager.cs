//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.CadPlus.XToolbar.Enums;
using Xarial.CadPlus.XToolbar.Helpers;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.XToolbar.Services
{
    public interface ITriggersManager : IDisposable
    {
    }

    public class TriggersManager : ITriggersManager
    {
        private readonly IXApplication m_App;
        private readonly Dictionary<Triggers_e, CommandMacroInfo[]> m_Triggers;
        private readonly IMacroRunner m_MacroRunner;
        private readonly IMessageService m_Msg;
        private readonly IXLogger m_Logger;
        private readonly ICommandsManager m_CmdMgr;

        public TriggersManager(ICommandsManager cmdMgr, IXApplication app, 
            IMacroRunner macroRunner, IMessageService msgSvc, IXLogger logger)
        {
            m_CmdMgr = cmdMgr;
            m_App = app;
            m_MacroRunner = macroRunner;
            m_Msg = msgSvc;
            m_Logger = logger;

            m_Triggers = LoadTriggers(m_CmdMgr.ToolbarInfo);

            m_App.Documents.DocumentCreated += OnDocumentCreated;

            if (m_Triggers.Keys.Contains(Triggers_e.DocumentActivated))
            {
                m_App.Documents.DocumentActivated += OnDocumentActivated;
            }

            InvokeTrigger(Triggers_e.ApplicationStart);
        }

        private void OnDocumentActivated(IXDocument doc)
        {
            InvokeTrigger(Triggers_e.DocumentActivated);
        }

        private void OnDocumentCreated(IXDocument doc)
        {
            foreach (var trigger in m_Triggers.Keys)
            {
                switch (trigger)
                {
                    case Triggers_e.DocumentSave:
                        doc.Saving += OnSaving;
                        break;
                    case Triggers_e.NewSelection:
                        doc.Selections.NewSelection += OnNewSelection;
                        break;
                    case Triggers_e.ConfigurationChange:
                        switch (doc) 
                        {
                            case IXDocument3D doc3D:
                                doc3D.Configurations.ConfigurationActivated += OnConfigurationActivated;
                                break;
                            case IXDrawing draw:
                                draw.Sheets.SheetActivated += OnSheetActivated;
                                break;
                        }
                        break;
                    case Triggers_e.Rebuild:
                        doc.Rebuild += OnRebuild;
                        break;
                }
            }

            if (doc == m_App.Documents.Active)
            {
                if (!string.IsNullOrEmpty(doc.Path))
                {
                    InvokeTrigger(Triggers_e.DocumentOpen);
                }
                else
                {
                    InvokeTrigger(Triggers_e.DocumentNew);
                }
            }

            doc.Closing += OnDocumentClosing;
        }

        private void OnDocumentClosing(IXDocument doc)
        {
            if (doc.Visible)
            {
                InvokeTrigger(Triggers_e.DocumentClose);
            }

            switch (doc)
            {
                case IXDocument3D doc3D:
                    doc3D.Configurations.ConfigurationActivated -= OnConfigurationActivated;
                    break;
                case IXDrawing draw:
                    draw.Sheets.SheetActivated -= OnSheetActivated;
                    break;
            }

            doc.Selections.NewSelection -= OnNewSelection;
            doc.Saving -= OnSaving;
            doc.Rebuild -= OnRebuild;
            doc.Closing -= OnDocumentClosing;
        }

        private void OnRebuild(IXDocument doc)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.Rebuild);
            }
        }

        private void OnSheetActivated(IXDrawing doc, IXSheet newSheet)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.ConfigurationChange);
            }
        }

        private void OnConfigurationActivated(IXDocument3D doc, IXConfiguration newConf)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.ConfigurationChange);
            }
        }

        private void OnNewSelection(IXDocument doc, IXSelObject selObject)
        {
            InvokeTrigger(Triggers_e.NewSelection);
        }

        private void OnSaving(IXDocument doc, DocumentSaveType_e type,
            DocumentSaveArgs args)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.DocumentSave);
            }
        }

        private Dictionary<Triggers_e, CommandMacroInfo[]> LoadTriggers(CustomToolbarInfo toolbarInfo)
        {
            var triggersCmds = new Dictionary<Triggers_e, CommandMacroInfo[]>();

            var allCmds = toolbarInfo?.Groups?.SelectMany(g => g.Commands);

            if (allCmds?.Any() == true)
            {
                var triggers = typeof(Triggers_e).GetEnumFlags().Where(e => !e.Equals(Triggers_e.Button));

                foreach (Triggers_e trigger in triggers)
                {
                    var cmds = allCmds.Where(c => c.Triggers.HasFlag(trigger));

                    if (cmds.Any())
                    {
                        triggersCmds.Add(trigger, cmds.ToArray());
                    }
                }
            }

            return triggersCmds;
        }
        
        private void InvokeTrigger(Triggers_e trigger)
        {
            CommandMacroInfo[] cmds;

            if (m_Triggers.TryGetValue(trigger, out cmds))
            {
                cmds = cmds.Where(c => c.Scope.IsInScope(m_App)).ToArray();

                if (cmds != null && cmds.Any())
                {
                    m_Logger.Log($"Invoking {cmds.Length} command(s) for the trigger {trigger}");

                    foreach (var cmd in cmds)
                    {
                        try
                        {
                            m_MacroRunner.RunMacro(cmd.MacroPath, cmd.EntryPoint, false);
                        }
                        catch(Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_Msg.ShowError(ex, $"Failed to run a macro on trigger: {trigger}");
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            m_App.Documents.DocumentCreated -= OnDocumentCreated;
            m_App.Documents.DocumentActivated -= OnDocumentActivated;
        }
    }
}
