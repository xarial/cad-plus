//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Helpers;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface ICommandsManager : IDisposable
    {
        void CreateCommandGroups(CustomToolbarInfo toolbarInfo, string workDir);
    }

    public partial class CommandsManager : ICommandsManager
    {
        private readonly IXExtension m_AddIn;
        private readonly IXApplication m_App;
        private readonly IMacroRunner m_MacroRunner;
        private readonly IMessageService m_Msg;
        private readonly IXLogger m_Logger;
        private readonly IIconsProvider[] m_IconsProviders;

        private readonly Dictionary<CommandMacroInfo, bool> m_CachedToggleStates;
        private readonly ConcurrentDictionary<CommandMacroInfo, IToggleButtonStateResolver> m_StateResolvers;
        private readonly IFilePathResolver m_PathResolver;

        private string m_WorkDir;

        public CommandsManager(IXExtension addIn, IXApplication app,
            IMacroRunner macroRunner, IMessageService msg,
            IXLogger logger, IIconsProvider[] iconsProviders, IFilePathResolver pathResolver)
        {
            m_AddIn = addIn;
            m_App = app;
            m_MacroRunner = macroRunner;
            m_Msg = msg;
            m_Logger = logger;
            m_IconsProviders = iconsProviders;

            m_CachedToggleStates = new Dictionary<CommandMacroInfo, bool>();
            m_StateResolvers = new ConcurrentDictionary<CommandMacroInfo, IToggleButtonStateResolver>();
            m_PathResolver = pathResolver;
        }

        public void CreateCommandGroups(CustomToolbarInfo toolbarInfo, string workDir) 
        {
            const string COMMAND_GROUP_TITLE_TEMPLATE = "Toolbar+ Command Group";
            const string COMMAND_TITLE_TEMPLATE = "Toolbar+ Command";

            m_WorkDir = workDir;

            var usedCommandGroupNames = new List<string>();
            var usedCommandNames = new List<string>();

            if (toolbarInfo?.Groups != null)
            {
                foreach (var grp in toolbarInfo.Groups
                    .Where(g => g.Commands?.Any(c => c.Triggers.HasFlag(Triggers_e.Button)) == true))
                {
                    var cmdGrp = new CommandGroupInfoSpec(grp, m_IconsProviders, m_PathResolver, m_WorkDir);
                    
                    ResolveEmptyName(cmdGrp.Info, COMMAND_GROUP_TITLE_TEMPLATE, usedCommandGroupNames, out string grpTitle, out string grpTooltip);
                    cmdGrp.Title = grpTitle;
                    cmdGrp.Tooltip = grpTooltip;

                    foreach (var cmd in cmdGrp.Commands)
                    {
                        ResolveEmptyName(((CommandItemInfoSpec)cmd).Info, COMMAND_TITLE_TEMPLATE, usedCommandNames, out string cmdTitle, out string cmdTooltip);
                        cmd.Title = cmdTitle;
                        cmd.Tooltip = cmdTooltip;
                    }

                    m_Logger.Log($"Adding command group: {cmdGrp.Title} [{cmdGrp.Id}]. Commands: {string.Join(", ", cmdGrp.Commands.Select(c => $"{c.Title} [{c.UserId}]").ToArray())}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                    var cmdGrpCad = m_AddIn.CommandManager.AddCommandGroup(cmdGrp);

                    cmdGrpCad.CommandClick += OnCommandClick;
                    cmdGrpCad.CommandStateResolve += OnCommandStateResolve;
                }

                LoadToggleStateResolvers(
                    toolbarInfo.Groups.SelectMany(
                        g => g.Commands ?? Enumerable.Empty<CommandMacroInfo>())
                    .Where(m => m.Triggers.HasFlag(Triggers_e.ToggleButton) && m.ToggleButtonStateCodeType != ToggleButtonStateCode_e.None));
            }
        }
        
        private async void LoadToggleStateResolvers(IEnumerable<CommandMacroInfo> toggleMacros) 
        {
            try
            {
                await Task.Run(() => CompileStateResolveCodeCode(toggleMacros));
            }
            catch (Exception ex)
            {
                m_Logger.Log($"Toggle state code compilation errors", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
                m_Logger.Log(ex);

                if (ex is ReflectionTypeLoadException) 
                {
                    var loaderExs = (ex as ReflectionTypeLoadException).LoaderExceptions;

                    if (loaderExs != null) 
                    {
                        foreach (var loaderEx in loaderExs) 
                        {
                            m_Logger.Log(loaderEx);
                        }
                    }
                }

                m_Msg.ShowError($"Failed to compile the toggle state code");
            }
        }

        private void ResolveEmptyName(CommandItemInfo info, string nameTemplate, List<string> usedNames, out string title, out string tooltip)
        {
            title = info.Title;
            tooltip = info.Description;

            if (string.IsNullOrEmpty(title))
            {
                title = nameTemplate;
                int i = 0;

                while (usedNames.Contains(title, StringComparer.CurrentCultureIgnoreCase))
                {
                    title = $"{nameTemplate}{++i}";
                }

                usedNames.Add(title);
            }

            if (string.IsNullOrEmpty(tooltip))
            {
                tooltip = title;
            }
        }

        private void CompileStateResolveCodeCode(IEnumerable<CommandMacroInfo> macroInfos)
        {
            foreach (var grp in macroInfos.GroupBy(x => x.ToggleButtonStateCodeType))
            {
                IStateResolveCompiler compiler = null;
                switch (grp.Key)
                {
                    //case ToggleButtonStateCode_e.CSharp:
                    //    compiler = new CSharpStateResolveCompiler(Settings.Default.ToggleButtonResolverCSharp, m_App);
                    //    break;
                    case ToggleButtonStateCode_e.VBNET:
                        compiler = new VbNetStateResolveCompiler(Settings.Default.ToggleButtonResolverVBNET, m_App);
                        break;
                    default:
                        throw new NotSupportedException("Not supported language");
                }

                foreach (var macroInfoResolverPair in compiler.CreateResolvers(grp)) 
                {
                    m_StateResolvers.TryAdd(macroInfoResolverPair.Key, macroInfoResolverPair.Value);
                }
            }
        }
        
        private void OnCommandStateResolve(CommandSpec spec, CommandState state)
        {
            var cmdSpec = (CommandItemInfoSpec)spec;
            state.Enabled = cmdSpec.Info.Scope.IsInScope(m_App);

            if (state.Enabled)
            {
                if (cmdSpec.Info.Triggers.HasFlag(Triggers_e.ToggleButton))
                {
                    state.Checked = TryResolveState(cmdSpec.Info);
                }
            }
        }

        private void OnCommandClick(CommandSpec spec)
        {
            var cmdSpec = (CommandItemInfoSpec)spec;
            var macroInfo = cmdSpec.Info;

            var trigger = macroInfo.Triggers.HasFlag(Triggers_e.ToggleButton) ? Triggers_e.ToggleButton : Triggers_e.Button;

            var targetDoc = m_App.Documents.Active;

            if (m_MacroRunner.TryRunMacroCommand(trigger, macroInfo, targetDoc, m_WorkDir))
            {
                if (macroInfo.Triggers.HasFlag(Triggers_e.ToggleButton))
                {
                    if (macroInfo.ResolveButtonStateCodeOnce || macroInfo.ToggleButtonStateCodeType == ToggleButtonStateCode_e.None)
                    {
                        if (m_CachedToggleStates.TryGetValue(macroInfo, out bool isChecked))
                        {
                            isChecked = !isChecked;
                            m_CachedToggleStates[macroInfo] = isChecked;
                        }
                        else
                        {
                            Debug.Assert(false, "State should have been resolved by SOLIDWORKS before this call");
                        }
                    }
                }
            }
        }
        
        private bool TryResolveState(CommandMacroInfo macroInfo) 
        {
            bool isChecked;

            try
            {
                if (macroInfo.ResolveButtonStateCodeOnce || macroInfo.ToggleButtonStateCodeType == ToggleButtonStateCode_e.None)
                {
                    if (!m_CachedToggleStates.TryGetValue(macroInfo, out isChecked))
                    {
                        GetCheckState(macroInfo);
                        m_CachedToggleStates.Add(macroInfo, isChecked);
                    }
                }
                else
                {
                    isChecked = GetCheckState(macroInfo);
                }
            }
            catch 
            {
                isChecked = false;
            }

            return isChecked;
        }
        
        private bool GetCheckState(CommandMacroInfo macroInfo)
        {
            if (macroInfo.ToggleButtonStateCodeType == ToggleButtonStateCode_e.None)
            {
                return false;
            }
            else
            {
                try
                {
                    IToggleButtonStateResolver stateResolver = null;

                    m_StateResolvers.TryGetValue(macroInfo, out stateResolver);

                    if (stateResolver != null)
                    {
                        return stateResolver.Resolve();
                    }
                    else 
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    m_Logger.Log(ex);
                    return false;
                }
            }
        }
        
        public void Dispose()
        {
        }
    }
}
