//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Helpers;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface ITriggersManager : IDisposable
    {
    }

    public class TriggersManager : ITriggersManager
    {
        private readonly IXApplication m_App;
        private readonly Dictionary<Triggers_e, CommandMacroInfo[]> m_Triggers;
        private readonly IMacroRunner m_MacroRunner;
        private readonly IXLogger m_Logger;
        private readonly ICommandsManager m_CmdMgr;

        public TriggersManager(ICommandsManager cmdMgr, IXApplication app, 
            IMacroRunner macroRunner, IXLogger logger)
        {
            m_CmdMgr = cmdMgr;
            m_App = app;
            m_MacroRunner = macroRunner;
            m_Logger = logger;

            m_Triggers = LoadTriggers(m_CmdMgr.ToolbarInfo);

            m_App.Documents.DocumentLoaded += OnDocumentLoaded;

            if (m_Triggers.Keys.Contains(Triggers_e.DocumentOpen)) 
            {
                m_App.Documents.DocumentOpened += OnDocumentOpened;
            }

            if (m_Triggers.Keys.Contains(Triggers_e.DocumentNew))
            {
                m_App.Documents.NewDocumentCreated += OnNewDocumentCreated;
            }

            if (m_Triggers.Keys.Contains(Triggers_e.DocumentActivated))
            {
                m_App.Documents.DocumentActivated += OnDocumentActivated;
            }

            InvokeTrigger(Triggers_e.ApplicationStart, null);
        }

        private void OnDocumentOpened(IXDocument doc)
            => InvokeTrigger(Triggers_e.DocumentOpen, doc);

        private void OnNewDocumentCreated(IXDocument doc)
            => InvokeTrigger(Triggers_e.DocumentNew, doc);

        private void OnDocumentActivated(IXDocument doc)
            => InvokeTrigger(Triggers_e.DocumentActivated, doc);

        private void OnDocumentLoaded(IXDocument doc)
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
                        doc.Rebuilt += OnRebuild;
                        break;
                }
            }

            doc.Closing += OnDocumentClosing;
        }

        private void OnDocumentClosing(IXDocument doc)
        {
            if (!doc.State.HasFlag(DocumentState_e.Hidden))
            {
                InvokeTrigger(Triggers_e.DocumentClose, doc);
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
            doc.Rebuilt -= OnRebuild;
            doc.Closing -= OnDocumentClosing;
        }

        private void OnRebuild(IXDocument doc)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.Rebuild, doc);
            }
        }

        private void OnSheetActivated(IXDrawing doc, IXSheet newSheet)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.ConfigurationChange, doc);
            }
        }

        private void OnConfigurationActivated(IXDocument3D doc, IXConfiguration newConf)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.ConfigurationChange, doc);
            }
        }

        private void OnNewSelection(IXDocument doc, IXSelObject selObject)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.NewSelection, doc);
            }
        }

        private void OnSaving(IXDocument doc, DocumentSaveType_e type,
            DocumentSaveArgs args)
        {
            if (doc == m_App.Documents.Active)
            {
                InvokeTrigger(Triggers_e.DocumentSave, doc);
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
        
        private void InvokeTrigger(Triggers_e trigger, IXDocument targetDoc)
        {
            CommandMacroInfo[] cmds;

            if (m_Triggers.TryGetValue(trigger, out cmds))
            {
                cmds = cmds.Where(c => c.Scope.IsInScope(m_App)).ToArray();

                if (cmds != null && cmds.Any())
                {
                    m_Logger.Log($"Invoking {cmds.Length} command(s) for the trigger {trigger}", LoggerMessageSeverity_e.Debug);

                    foreach (var cmd in cmds)
                    {
                        m_MacroRunner.TryRunMacroCommand(trigger, cmd, targetDoc);
                    }
                }
            }
        }

        public void Dispose()
        {
            m_App.Documents.DocumentLoaded -= OnDocumentLoaded;
            m_App.Documents.DocumentActivated -= OnDocumentActivated;
            m_App.Documents.DocumentOpened -= OnDocumentOpened;
            m_App.Documents.NewDocumentCreated -= OnNewDocumentCreated;
        }
    }
}
