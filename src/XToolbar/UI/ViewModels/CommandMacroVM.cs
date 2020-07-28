//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.Windows.Input;
using Xarial.CadPlus.XToolbar.Enums;
using Xarial.CadPlus.XToolbar.Structs;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.XToolbar.UI.ViewModels
{
    public class CommandMacroVM : CommandVM<CommandMacroInfo>, INotifyPropertyChanged
    {
        private ICommand m_BrowseMacroPathCommand;

        public string MacroPath
        {
            get
            {
                return Command.MacroPath;
            }
            set
            {
                Command.MacroPath = value;
                this.NotifyChanged();
            }
        }

        public MacroStartFunction EntryPoint
        {
            get
            {
                return Command.EntryPoint;
            }
            set
            {
                Command.EntryPoint = value;
                this.NotifyChanged();
            }
        }

        public ICommand BrowseMacroPathCommand
        {
            get
            {
                if (m_BrowseMacroPathCommand == null)
                {
                    m_BrowseMacroPathCommand = new RelayCommand(() =>
                    {
                        if (FileSystemBrowser.BrowseFileOpen(out string macroFile, 
                            "Select macro file",
                            FileSystemBrowser.BuildFilterString(
                                new FileFilter(
                                    "SOLIDWORKS Macros", "*.swp", "*.swb", "*.dll"), 
                                FileFilter.AllFiles)))
                        {
                            MacroPath = macroFile;
                        }
                    });
                }

                return m_BrowseMacroPathCommand;
            }
        }

        public MacroScope_e Scope
        {
            get
            {
                return Command.Scope;
            }
            set
            {
                Command.Scope = value;
                this.NotifyChanged();
            }
        }

        public Triggers_e Triggers
        {
            get
            {
                return Command.Triggers;
            }
            set
            {
                Command.Triggers = value;
                this.NotifyChanged();
            }
        }

        public CommandMacroVM() : this(new CommandMacroInfo())
        {
        }

        public CommandMacroVM(CommandMacroInfo cmd) : base(cmd)
        {
        }
    }
}