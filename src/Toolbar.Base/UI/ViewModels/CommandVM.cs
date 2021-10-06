//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public interface ICommandVM
    {
        string WorkingDirectory { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string IconPath { get; set; }
        BitmapSource Icon { get; }
        ICommand BrowseIconCommand { get; }
        CommandItemInfo Command { get; }
    }

    public abstract class CommandVM<TCmdInfo> : INotifyPropertyChanged, ICommandVM
        where TCmdInfo : CommandItemInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ICommand m_BrowseIconCommand;

        internal TCmdInfo Command { get; }

        public string Title
        {
            get
            {
                return Command.Title;
            }
            set
            {
                Command.Title = value;
                this.NotifyChanged();
            }
        }

        public string Description
        {
            get
            {
                return Command.Description;
            }
            set
            {
                Command.Description = value;
                this.NotifyChanged();
            }
        }

        public string IconPath
        {
            get
            {
                return Command.IconPath;
            }
            set
            {
                if (!string.Equals(Command.IconPath, value))
                {
                    Command.IconPath = value;
                    UpdateIcon();
                }
                this.NotifyChanged();
            }
        }

        public ICommand BrowseIconCommand
        {
            get
            {
                if (m_BrowseIconCommand == null)
                {
                    m_BrowseIconCommand = new RelayCommand(() =>
                    {
                        var filters = m_IconProviders.Select(p => new FileFilter(p.Filter.Name, p.Filter.Extensions))
                            .Union(new FileFilter[] { FileFilter.AllFiles }).ToArray();

                        if (FileSystemBrowser.BrowseFileOpen(out string imgFile,
                            "Select image file for icon",
                            FileSystemBrowser.BuildFilterString(filters))) 
                        {
                            IconPath = imgFile;
                        }
                    });
                }

                return m_BrowseIconCommand;
            }
        }

        CommandItemInfo ICommandVM.Command => Command;

        public string WorkingDirectory
        {
            get => m_WorkingDirectory;
            set 
            {
                if (!string.Equals(m_WorkingDirectory, value))
                {
                    m_WorkingDirectory = value;
                    UpdateIcon();
                }
            }
        }

        public BitmapSource Icon
        {
            get => m_Icon;
            private set
            {
                m_Icon = value;
                this.NotifyChanged();
            }
        }

        private string m_WorkingDirectory;
        private BitmapSource m_Icon;
        private readonly IIconsProvider[] m_IconProviders;
        private readonly IFilePathResolver m_FilePathResolver;

        protected CommandVM(TCmdInfo cmd, IIconsProvider[] providers, IFilePathResolver filePathResolver)
        {
            Command = cmd;
            m_IconProviders = providers;
            m_FilePathResolver = filePathResolver;
        }

        private void UpdateIcon() 
        {
            var iconPath = IconPath;
            var workDir = WorkingDirectory;

            BitmapSource icon = null;

            try
            {
                iconPath = m_FilePathResolver.Resolve(iconPath, workDir);

                if (File.Exists(iconPath))
                {
                    var provider = m_IconProviders.FirstOrDefault(p => p.Matches(iconPath));

                    if (provider != null)
                    {
                        icon = provider.GetThumbnail(iconPath).ToBitmapImage();
                    }
                }
            }
            catch
            {
            }

            if (icon == null)
            {
                icon = DefaultIcon;
            }

            Icon = icon;
        }

        protected abstract BitmapSource DefaultIcon { get; }
    }
}