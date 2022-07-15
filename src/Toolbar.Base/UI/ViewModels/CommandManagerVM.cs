﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.CustomToolbar.UI.Base;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XCad.Base;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public class CommandManagerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ICommandVM m_SelectedElement;
        private ICommand m_BrowseToolbarSpecificationCommand;

        private ICommand m_MoveCommandUpCommand;
        private ICommand m_MoveCommandDownCommand;
        private ICommand m_InsertCommandAfterCommand;
        private ICommand m_InsertCommandBeforeCommand;
        private ICommand m_CommandRemoveCommand;
        private ICommand m_MacroDropCommand;

        private CommandsCollection<CommandGroupVM> m_Groups;

        private readonly IToolbarConfigurationProvider m_ConfsProvider;
        private readonly IMessageService m_MsgService;
        private readonly IXLogger m_Logger;

        private string m_ToolbarSpecificationPath;
        private bool m_IsEditable;

        private readonly ICommandGroupVMFactory m_CmdGrpFact;
        private readonly string[] m_MacroExtensions;

        public CommandManagerVM(IToolbarConfigurationProvider confsProvider,
            IMessageService msgService, IXLogger logger, ICadDescriptor cadEntDesc, ICommandGroupVMFactory cmdGrpFact)
        {
            m_ConfsProvider = confsProvider;
            m_MsgService = msgService;
            m_Logger = logger;

            HelpCommand = new RelayCommand(Help);

            m_MacroExtensions = cadEntDesc.MacroFileFilters
                .Select(f => f.Extensions)
                .SelectMany(x => x)
                .Union(XCadMacroProvider.Filter.Extensions)
                .Select(x => Path.GetExtension(x))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            m_CmdGrpFact = cmdGrpFact;
        }

        private void Help()
        {
            try
            {
                Process.Start("https://cadplus.xarial.com/toolbar/");
            }
            catch
            {
            }
        }

        public void Load(CustomToolbarInfo toolbarInfo, string toolbarPath)
        {
            ToolbarInfo = toolbarInfo;
            m_ToolbarSpecificationPath = toolbarPath;
            IsEditable = !m_ConfsProvider.IsReadOnly(toolbarPath);

            LoadToolbar(ToolbarInfo);
        }

        private void LoadToolbar(CustomToolbarInfo toolbarInfo)
        {
            if (Groups != null)
            {
                Groups.CommandsChanged -= OnGroupsCollectionChanged;
                Groups.NewCommandCreated -= OnNewCommandCreated;
            }

            Groups = new CommandsCollection<CommandGroupVM>(
                (toolbarInfo.Groups ?? new CommandGroupInfo[0])
                .Select(g => m_CmdGrpFact.Create(g)),
                () => m_CmdGrpFact.Create(new CommandGroupInfo()));

            HandleCommandGroupCommandCreation(Groups.Commands);

            Groups.NewCommandCreated += OnNewCommandCreated;
            Groups.CommandsChanged += OnGroupsCollectionChanged;
        }

        private void OnNewCommandCreated(ICommandVM cmd)
        {
            SelectedElement = cmd;
            cmd.WorkingDirectory = WorkingDirectory;
        }

        public bool IsEditable
        {
            get
            {
                return m_IsEditable;
            }
            private set
            {
                m_IsEditable = value;
                this.NotifyChanged();
            }
        }

        public CustomToolbarInfo ToolbarInfo { get; private set; }

        public CommandsCollection<CommandGroupVM> Groups
        {
            get
            {
                return m_Groups;
            }
            private set
            {
                m_Groups = value;
                this.NotifyChanged();
            }
        }

        public ICommandVM SelectedElement
        {
            get
            {
                return m_SelectedElement;
            }
            set
            {
                m_SelectedElement = value;
                this.NotifyChanged();
            }
        }

        public string ToolbarSpecificationPath
        {
            get => m_ToolbarSpecificationPath;
            set
            {
                m_ToolbarSpecificationPath = value;

                Load(m_ConfsProvider.GetToolbar(m_ToolbarSpecificationPath), m_ToolbarSpecificationPath);

                this.NotifyChanged();
            }
        }

        public ICommand BrowseToolbarSpecificationCommand
        {
            get
            {
                if (m_BrowseToolbarSpecificationCommand == null)
                {
                    m_BrowseToolbarSpecificationCommand = new RelayCommand(() =>
                    {
                        if (FileSystemBrowser.BrowseFileOpen(out string specFile,
                            "Select toolbar specification file",
                            FileSystemBrowser.BuildFilterString(
                                new FileFilter("Toolbar Specification File", "*.setts"))))
                        {
                            if (!string.Equals(specFile, ToolbarSpecificationPath, StringComparison.CurrentCultureIgnoreCase))
                            {
                                try
                                {
                                    ToolbarSpecificationPath = specFile;
                                }
                                catch (Exception ex)
                                {
                                    m_Logger.Log(ex);
                                    m_MsgService.ShowError(ex, "Failed to load toolbar information from the specified file");
                                }
                            }
                        }
                    });
                }

                return m_BrowseToolbarSpecificationCommand;
            }
        }

        public ICommand MoveCommandUpCommand
        {
            get
            {
                if (m_MoveCommandUpCommand == null)
                {
                    m_MoveCommandUpCommand = new RelayCommand<ICommandVM>(x =>
                    {
                        try
                        {
                            MoveCommand(x, false);
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_MsgService.ShowError(ex, "Failed to move command to this position");
                        }
                    });
                }

                return m_MoveCommandUpCommand;
            }
        }

        public ICommand MoveCommandDownCommand
        {
            get
            {
                if (m_MoveCommandDownCommand == null)
                {
                    m_MoveCommandDownCommand = new RelayCommand<ICommandVM>(x =>
                    {
                        try
                        {
                            MoveCommand(x, true);
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_MsgService.ShowError(ex, "Failed to move command to this position");
                        }
                    });
                }

                return m_MoveCommandDownCommand;
            }
        }

        public ICommand InsertCommandAfterCommand
        {
            get
            {
                if (m_InsertCommandAfterCommand == null)
                {
                    m_InsertCommandAfterCommand = new RelayCommand<ICommandVM>(x =>
                    {
                        try
                        {
                            InsertNewCommand(x, true);
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_MsgService.ShowError(ex, "Failed to move insert new command in this position");
                        }
                    });
                }

                return m_InsertCommandAfterCommand;
            }
        }

        public ICommand InsertCommandBeforeCommand
        {
            get
            {
                if (m_InsertCommandBeforeCommand == null)
                {
                    m_InsertCommandBeforeCommand = new RelayCommand<ICommandVM>(x =>
                    {
                        try
                        {
                            InsertNewCommand(x, false);
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_MsgService.ShowError(ex, "Failed to move insert new command in this position");
                        }
                    });
                }

                return m_InsertCommandBeforeCommand;
            }
        }

        public ICommand CommandRemoveCommand
        {
            get
            {
                if (m_CommandRemoveCommand == null)
                {
                    m_CommandRemoveCommand = new RelayCommand<ICommandVM>(x =>
                    {
                        try
                        {
                            RemoveCommand(x);
                        }
                        catch (Exception ex)
                        {
                            m_Logger.Log(ex);
                            m_MsgService.ShowError(ex, "Failed to remove command");
                        }
                    });
                }

                return m_CommandRemoveCommand;
            }
        }

        public ICommand MacroDropCommand
        {
            get
            {
                if (m_MacroDropCommand == null)
                {
                    m_MacroDropCommand = new RelayCommand<Views.CommandManagerView.MacroDropArgs>(a =>
                    {
                        ICommandsCollection targetColl = null;
                        int index = -1;

                        if (a.TargetCommand is CommandGroupVM)
                        {
                            targetColl = (a.TargetCommand as CommandGroupVM).Commands;
                            index = 0;
                        }
                        else if (a.TargetCommand == null)
                        {
                            var newGrp = (CommandGroupVM)Groups.AddNewCommand(Groups.Commands.Count);
                            targetColl = newGrp.Commands;
                            index = 0;
                        }
                        else if (a.TargetCommand is CommandMacroVM)
                        {
                            index = CalculateCommandIndex(a.TargetCommand, true, out targetColl);
                        }
                        else
                        {
                            throw new NotSupportedException("Invalid drop argument");
                        }

                        for (int i = 0; i < a.FilePaths.Length; i++)
                        {
                            var cmd = (CommandMacroVM)targetColl.AddNewCommand(index + i);
                            cmd.MacroPath = a.FilePaths[i];
                            cmd.Title = Path.GetFileNameWithoutExtension(a.FilePaths[i]);
                        }
                    },
                    a => IsValidFilesDrop(a.FilePaths));
                }

                return m_MacroDropCommand;
            }
        }

        public ICommand HelpCommand { get; }

        private bool IsValidFilesDrop(string[] files)
            => files?.All(f => m_MacroExtensions
            .Contains(Path.GetExtension(f), StringComparer.CurrentCultureIgnoreCase)) == true;

        private void MoveCommand(ICommandVM cmd, bool forward)
        {
            ICommandsCollection coll;

            var index = CalculateCommandIndex(cmd, forward, out coll);

            var cmds = coll.Commands;

            if (index < 0 || index >= cmds.Count)
            {
                throw new IndexOutOfRangeException("Index is outside the boundaries of the commands collection");
            }

            cmds.Remove(cmd);
            cmds.Insert(index, cmd);
            SelectedElement = cmd;
        }

        private void InsertNewCommand(ICommandVM cmd, bool after)
        {
            ICommandsCollection coll;

            var index = CalculateCommandIndex(cmd, after, out coll);

            if (!after)
            {
                index++;//insert to current position
            }

            coll.AddNewCommand(index);
        }

        private void RemoveCommand(ICommandVM cmd)
        {
            var coll = FindCommandCollection(cmd);
            coll.Commands.Remove(cmd);

            if (coll.Commands.Count > 0)
            {
                SelectedElement = coll.Commands[0] as ICommandVM;
            }
            else
            {
                SelectedElement = null;
            }
        }

        private int CalculateCommandIndex(ICommandVM cmd, bool forward, out ICommandsCollection coll)
        {
            var offset = forward ? 1 : -1;
            coll = FindCommandCollection(cmd);

            var index = coll.Commands.IndexOf(cmd);

            if (index == -1)
            {
                throw new IndexOutOfRangeException("Index of the command is not found");
            }

            return index + offset;
        }

        private ICommandsCollection FindCommandCollection(ICommandVM targetCmd)
        {
            foreach (var grp in Groups.Commands)
            {
                if (grp == targetCmd)
                {
                    return Groups;
                }
                else
                {
                    foreach (var cmd in grp.Commands.Commands)
                    {
                        if (cmd == targetCmd)
                        {
                            return grp.Commands;
                        }
                    }
                }
            }

            throw new NullReferenceException("Failed to find the command");
        }

        private void OnGroupsCollectionChanged(IEnumerable<CommandGroupVM> grps)
        {
            ToolbarInfo.Groups = grps
                .Select(g => g.Command).ToArray();

            HandleCommandGroupCommandCreation(grps);
        }

        private void HandleCommandGroupCommandCreation(IEnumerable<CommandGroupVM> grps)
        {
            foreach (var grp in grps)
            {
                grp.Commands.NewCommandCreated -= OnNewCommandCreated;
                grp.Commands.NewCommandCreated += OnNewCommandCreated;
            }

            SetCommandGroupsWorkingDirectory(grps);
        }

        private string WorkingDirectory
        {
            get
            {
                try
                {
                    return Path.GetDirectoryName(ToolbarSpecificationPath);
                }
                catch //may be illegal path
                {
                    return "";
                }
            }
        }

        private void SetCommandGroupsWorkingDirectory(IEnumerable<CommandGroupVM> grps) 
        {
            if (grps != null)
            {
                var workDir = WorkingDirectory;

                foreach (var grp in grps)
                {
                    grp.WorkingDirectory = workDir;

                    var cmds = grp.Commands;

                    if (cmds != null)
                    {
                        foreach (var cmd in cmds)
                        {
                            cmd.WorkingDirectory = workDir;
                        }
                    }
                }
            }
        }
    }
}