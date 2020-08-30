//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Helpers;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface ICommandsManager : IDisposable
    {
        CustomToolbarInfo ToolbarInfo { get; }
        void UpdatedToolbarConfiguration(ToolbarSettings toolbarSets, CustomToolbarInfo toolbarConf, bool isEditable);
        void RunMacroCommand(CommandMacroInfo cmd);
    }

    public class CommandsManager : ICommandsManager
    {
        private readonly IXExtension m_AddIn;
        private readonly IXApplication m_App;
        private readonly IMacroRunner m_MacroRunner;
        private readonly IMessageService m_Msg;
        private readonly ISettingsProvider m_SettsProvider;
        private readonly IToolbarConfigurationProvider m_ToolbarConfProvider;
        private readonly IXLogger m_Logger;

        public CustomToolbarInfo ToolbarInfo { get; }

        public CommandsManager(IXExtension addIn, IXApplication app,
            IMacroRunner macroRunner,
            IMessageService msg, ISettingsProvider settsProvider,
            IToolbarConfigurationProvider toolbarConfProvider,
            IXLogger logger)
        {
            m_AddIn = addIn;
            m_App = app;
            m_MacroRunner = macroRunner;
            m_Msg = msg;
            m_SettsProvider = settsProvider;
            m_ToolbarConfProvider = toolbarConfProvider;
            m_Logger = logger;

            try
            {
                ToolbarInfo = LoadUserToolbar();
            }
            catch(Exception ex)
            {
                m_Msg.ShowError(ex, "Failed to load toolbar specification");
            }
        }

        public void RunMacroCommand(CommandMacroInfo cmd)
        {
            try
            {
                m_MacroRunner.RunMacro(cmd.MacroPath, cmd.EntryPoint, cmd.UnloadAfterRun);
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_Msg.ShowError(ex, $"Failed to run macro: '{cmd.Title}'");
            }
        }

        public void UpdatedToolbarConfiguration(ToolbarSettings toolbarSets, CustomToolbarInfo toolbarConf, bool isEditable)
        {
            bool isToolbarChanged;

            SaveSettingChanges(toolbarSets, toolbarConf, isEditable, out isToolbarChanged);

            if (isToolbarChanged)
            {
                m_Msg.ShowMessage("Toolbar specification has changed. Please restart SOLIDWORKS",
                    MessageType_e.Info);
            }
        }

        private CustomToolbarInfo LoadUserToolbar()
        {
            bool isReadOnly;
            var toolbarInfo = m_ToolbarConfProvider.GetToolbar(out isReadOnly,
                ToolbarSpecificationFile);

            if (toolbarInfo?.Groups != null)
            {
                foreach (var grp in toolbarInfo.Groups
                    .Where(g => g.Commands?.Any(c => c.Triggers.HasFlag(Triggers_e.Button)) == true))
                {
                    var cmdGrp = new CommandGroupInfoSpec(grp);
                    
                    m_Logger.Log($"Adding command group: {cmdGrp.Title} [{cmdGrp.Id}]. Commands: {string.Join(", ", cmdGrp.Commands.Select(c => $"{c.Title} [{c.UserId}]").ToArray())}");

                    var cmdGrpCad = m_AddIn.CommandManager.AddCommandGroup(cmdGrp);

                    cmdGrpCad.CommandClick += OnCommandClick;
                    cmdGrpCad.CommandStateResolve += OnCommandStateResolve;
                }
            }

            return toolbarInfo;
        }

        private void OnCommandStateResolve(XCad.UI.Commands.Structures.CommandSpec spec, XCad.UI.Commands.Structures.CommandState state)
        {
            var cmdSpec = (CommandItemInfoSpec)spec;
            state.Enabled = cmdSpec.Info.Scope.IsInScope(m_App);
        }

        private void OnCommandClick(XCad.UI.Commands.Structures.CommandSpec spec)
        {
            var cmdSpec = (CommandItemInfoSpec)spec;
            RunMacroCommand(cmdSpec.Info);
        }
        
        private void SaveSettingChanges(ToolbarSettings toolbarSets, CustomToolbarInfo toolbarConf,
            bool isEditable, out bool isToolbarChanged)
        {
            isToolbarChanged = false;

            var oldToolbarSetts = m_SettsProvider.GetSettings();

            if (!DeepCompare(toolbarSets, oldToolbarSetts))
            {
                m_SettsProvider.SaveSettings(toolbarSets);
            }
            
            bool isReadOnly;

            var oldToolbarConf = m_ToolbarConfProvider
                .GetToolbar(out isReadOnly, oldToolbarSetts.SpecificationFile);

            isToolbarChanged = !DeepCompare(toolbarConf, oldToolbarConf);

            if (isToolbarChanged)
            {
                if (isEditable)
                {
                    UpdateGroupIds(toolbarConf.Groups, oldToolbarConf.Groups);
                    m_ToolbarConfProvider.SaveToolbar(toolbarConf, toolbarSets.SpecificationFile);
                }
                else
                {
                    m_Logger.Log("Skipped saving of read-only toolbar settings");
                }
            }
        }

        private void UpdateGroupIds(CommandGroupInfo[] curCmdGroups, CommandGroupInfo[] oldCmdGroups) 
        {
            var usedIds = curCmdGroups.Select(g => g.Id).ToList();

            int GetAvailableGroupId() 
            {
                int id = 1;

                while (usedIds.Contains(id)) 
                {
                    id++;
                }

                usedIds.Add(id);

                return id;
            }

            if (curCmdGroups != null && oldCmdGroups != null) 
            {
                foreach (var curCmdGrp in curCmdGroups) 
                {
                    var corrCmdGrp = oldCmdGroups.FirstOrDefault(g => g.Id == curCmdGrp.Id);

                    if (corrCmdGrp != null)
                    {
                        if (curCmdGrp.Commands != null && corrCmdGrp.Commands != null) 
                        {
                            if (!curCmdGrp.Commands.Select(c => c.Id).OrderBy(x => x)
                                .SequenceEqual(corrCmdGrp.Commands.Select(c => c.Id).OrderBy(x => x))) 
                            {
                                var newId = GetAvailableGroupId();
                                
                                m_Logger.Log($"Changing id of the group from {curCmdGrp.Id} to {newId}");

                                curCmdGrp.Id = newId;
                            }
                        }
                    }
                }
            }
        }

        private bool DeepCompare(object obj1, object obj2) 
            => JToken.DeepEquals(JToken.FromObject(obj1), JToken.FromObject(obj2));

        private string ToolbarSpecificationFile
        {
            get
            {
                return m_SettsProvider.GetSettings().SpecificationFile;
            }
        }

        public void Dispose()
        {
        }
    }
}
