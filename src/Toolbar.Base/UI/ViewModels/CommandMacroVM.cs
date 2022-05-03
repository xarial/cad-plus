//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.CustomToolbar.Enums;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public class CommandMacroVM : CommandVM<CommandMacroInfo>, INotifyPropertyChanged
    {
        private static readonly BitmapImage m_DefaultMacroIcon = Resources.macro_icon_default.ToBitmapImage();

        private ICommand m_BrowseMacroPathCommand;
        private MacroStartFunction[] m_EntryPoints;

        public string MacroPath
        {
            get => Command.MacroPath;
            set
            {
                Command.MacroPath = value;
                this.NotifyChanged();
                TryUpdateEntryPoints();
            }
        }

        public override string WorkingDirectory 
        {
            get => base.WorkingDirectory;
            set
            {
                base.WorkingDirectory = value;
                TryUpdateEntryPoints();
            }
        }

        public MacroStartFunction[] EntryPoints 
        {
            get => m_EntryPoints;
            private set 
            {
                m_EntryPoints = value;
                EntryPoint = EntryPoints?.FirstOrDefault();
                this.NotifyChanged();
            }
        }

        private void TryUpdateEntryPoints() 
        {
            try
            {
                if (!string.IsNullOrEmpty(MacroPath))
                {
                    EntryPoints = m_Extractor.GetEntryPoints(MacroPath, WorkingDirectory);
                }
            }
            catch
            {
                EntryPoints = null;
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

        public string Arguments
        {
            get => Command.Arguments;
            set
            {
                Command.Arguments = value;
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
                            "Select macro file", FileSystemBrowser.BuildFilterString(m_MacroFileFilters)))
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

        protected override BitmapSource DefaultIcon => m_DefaultMacroIcon;

        private readonly FileFilter[] m_MacroFileFilters;
        private readonly IMacroEntryPointsExtractor m_Extractor;

        public CommandMacroVM() : this(new CommandMacroInfo(), ToolbarModule.Resolve<IIconsProvider[]>(), ToolbarModule.Resolve<IFilePathResolver>(), ToolbarModule.Resolve<IMacroEntryPointsExtractor>())
        {
        }

        public CommandMacroVM(CommandMacroInfo cmd, IIconsProvider[] providers, IFilePathResolver filePathResolver, IMacroEntryPointsExtractor extractor)
            : base(cmd, providers, filePathResolver)
        {
            m_MacroFileFilters = ToolbarModule.Resolve<ICadDescriptor>().MacroFileFilters
                .Select(f => new FileFilter(f.Name, f.Extensions))
                .Union(new FileFilter[] { XCadMacroProvider.Filter, FileFilter.AllFiles }).ToArray();
            
            m_Extractor = extractor;
        }
    }
}