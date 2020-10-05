//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Exceptions;
using Xarial.CadPlus.CustomToolbar.Helpers;
using Xarial.CadPlus.CustomToolbar.Properties;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.ExtensionModule;
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
        bool RunMacroCommand(CommandMacroInfo cmd, out Exception err);
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

        private readonly Dictionary<CommandMacroInfo, bool> m_CachedToggleStates;
        private readonly Dictionary<CommandMacroInfo, IToggleBuggonStateResolver> m_CachedStateResolvers;

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

            m_CachedToggleStates = new Dictionary<CommandMacroInfo, bool>();
            m_CachedStateResolvers = new Dictionary<CommandMacroInfo, IToggleBuggonStateResolver>();

            try
            {
                ToolbarInfo = LoadUserToolbar();
            }
            catch(Exception ex)
            {
                m_Msg.ShowError(ex, "Failed to load toolbar specification");
            }
        }

        public bool RunMacroCommand(CommandMacroInfo cmd, out Exception err)
        {
            try
            {
                m_MacroRunner.RunMacro(cmd.MacroPath, cmd.EntryPoint, cmd.UnloadAfterRun);
                err = null;
                return true;
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                err = ex;
                return false;
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

            if (state.Enabled)
            {
                if (cmdSpec.Info.Triggers.HasFlag(Triggers_e.ToggleButton))
                {
                    state.Checked = TryResolveState(cmdSpec.Info);
                }
            }
        }

        private void OnCommandClick(XCad.UI.Commands.Structures.CommandSpec spec)
        {
            var cmdSpec = (CommandItemInfoSpec)spec;
            var macroInfo = cmdSpec.Info;

            if (RunMacroCommand(macroInfo, out Exception err)) 
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
            else
            {
                m_Msg.ShowError(err, $"Failed to run macro: '{cmdSpec.Title}'");
            }
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

        private bool TryResolveState(CommandMacroInfo macroInfo) 
        {
            bool isChecked = false;

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
                    IToggleBuggonStateResolver stateResolver = null;

                    if (!m_CachedStateResolvers.TryGetValue(macroInfo, out stateResolver))
                    {
                        var compilerDirPath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "roslyn");

                        var compilerFileName = "";

                        switch (macroInfo.ToggleButtonStateCodeType)
                        {
                            case ToggleButtonStateCode_e.CSharp:
                                compilerFileName = "csc.exe";
                                break;
                            case ToggleButtonStateCode_e.VBNET:
                                compilerFileName = "vbc.exe";
                                break;
                        }

                        var opts = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.ProviderOptions(
                            Path.Combine(compilerDirPath, compilerFileName), 0);
                        
                        CodeDomProvider codeProvider = null;

                        var className = "CT_" + Guid.NewGuid().ToString().TrimStart('{').TrimEnd('}').Replace("-", "");

                        var fullCode = "";
                        
                        switch (macroInfo.ToggleButtonStateCodeType)
                        {
                            case ToggleButtonStateCode_e.CSharp:
                                codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider(opts);
                                fullCode = string.Format(Settings.Default.ToggleButtonResolverCSharp, className, macroInfo.ToggleButtonStateCode);
                                break;

                            case ToggleButtonStateCode_e.VBNET:
                                codeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.VBCodeProvider(opts);
                                fullCode = string.Format(Settings.Default.ToggleButtonResolverVBNET, className, macroInfo.ToggleButtonStateCode);
                                break;

                            default:
                                throw new Exception("Code type is not supported");
                        }

                        var parameters = new CompilerParameters()
                        {
                            GenerateExecutable = false,
                            GenerateInMemory = true
                        };

                        parameters.ReferencedAssemblies.Add(typeof(IModule).Assembly.Location);
                        parameters.ReferencedAssemblies.Add(typeof(IXApplication).Assembly.Location);

                        var results = codeProvider.CompileAssemblyFromSource(parameters, fullCode);

                        if (!results.Errors.HasErrors)
                        {
                            var assm = results.CompiledAssembly;

                            Assembly AssemblyResolve(object sender, ResolveEventArgs args) 
                            {
                                var resolvedAssm = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(a => a.GetName().FullName == args.Name);

                                return resolvedAssm;
                            }

                            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

                            var stateResolverType = assm.GetTypes().First(t => typeof(IToggleBuggonStateResolver).IsAssignableFrom(t));
                            stateResolver = (IToggleBuggonStateResolver)Activator.CreateInstance(stateResolverType, m_App);

                            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;

                            m_CachedStateResolvers.Add(macroInfo, stateResolver);
                        }
                        else
                        {
                            throw new CustomResolverCodeCompileFailedException(results.Errors);
                        }
                    }

                    if (stateResolver != null)
                    {
                        return stateResolver.Resolve();
                    }
                    else 
                    {
                        return false;
                    }
                }
                catch (CustomResolverCodeCompileFailedException compileEx) 
                {
                    m_CachedStateResolvers.Add(macroInfo, null);
                    m_Logger.Log($"'{macroInfo.Title}' toggle state code compilation errors");
                    m_Logger.Log(compileEx);
                    m_Msg.ShowMessage($"Failed to compile the toggle state code for '{macroInfo.Title}'", MessageType_e.Error);
                    return false;
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
