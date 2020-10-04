//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.Windows.Input;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public class CommandMacroVM : CommandVM<CommandMacroInfo>, INotifyPropertyChanged
    {
        private ICommand m_BrowseMacroPathCommand;

        public string MacroPath
        {
            get => Command.MacroPath;
            set
            {
                Command.MacroPath = value;
                this.NotifyChanged();
            }
        }

        public MacroStartFunction EntryPoint
        {
            get => Command.EntryPoint;
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
                                    "SOLIDWORKS Macros", "*.swp", "*.swb", "*.dll"), //TODO: make the extensions list a dependency
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
            get => Command.Scope;
            set
            {
                Command.Scope = value;
                this.NotifyChanged();
            }
        }

        public Triggers_e Triggers
        {
            get => Command.Triggers;
            set
            {
                Command.Triggers = value;
                this.NotifyChanged();
            }
        }

        public bool UnloadAfterRun
        {
            get => Command.UnloadAfterRun;
            set
            {
                Command.UnloadAfterRun = value;
                this.NotifyChanged();
            }
        }

        public Location_e Location
        {
            get => Command.Location;
            set
            {
                Command.Location = value;
                this.NotifyChanged();
            }
        }

        public bool IsToggleButton 
        {
            get => Command.IsToggleButton;
            set 
            {
                Command.IsToggleButton = value;
                this.NotifyChanged();
            }
        }
        
        public ToggleButtonStateCode_e ToggleButtonStateCodeType
        {
            get => Command.ToggleButtonStateCodeType;
            set 
            {
                Command.ToggleButtonStateCodeType = value;
                this.NotifyChanged();
            }
        }
        
        public string ToggleButtonStateCode 
        {
            get => Command.ToggleButtonStateCode;
            set
            {
                Command.ToggleButtonStateCode = value;
                this.NotifyChanged();
            }
        }

        public bool ResolveButtonStateCodeOnce
        {
            get => Command.ResolveButtonStateCodeOnce;
            set
            {
                Command.ResolveButtonStateCodeOnce = value;
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