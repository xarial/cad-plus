//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.ComponentModel;
using System.Windows.Input;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public interface ICommandVM
    {
        string Title { get; set; }
        string Description { get; set; }
        string IconPath { get; set; }
        ICommand BrowseIconCommand { get; }
        CommandItemInfo Command { get; }
    }

    public abstract class CommandVM<TCmdInfo> : INotifyPropertyChanged, ICommandVM
        where TCmdInfo : CommandItemInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly TCmdInfo m_Command;
        private ICommand m_BrowseIconCommand;

        internal TCmdInfo Command
        {
            get
            {
                return m_Command;
            }
        }

        public string Title
        {
            get
            {
                return m_Command.Title;
            }
            set
            {
                m_Command.Title = value;
                this.NotifyChanged();
            }
        }

        public string Description
        {
            get
            {
                return m_Command.Description;
            }
            set
            {
                m_Command.Description = value;
                this.NotifyChanged();
            }
        }

        public string IconPath
        {
            get
            {
                return m_Command.IconPath;
            }
            set
            {
                m_Command.IconPath = value;
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
                        if (FileSystemBrowser.BrowseFileOpen(out string imgFile,
                            "Select image file for icon",
                            FileSystemBrowser.BuildFilterString(FileFilter.ImageFiles))) 
                        {
                            IconPath = imgFile;
                        }
                    });
                }

                return m_BrowseIconCommand;
            }
        }

        CommandItemInfo ICommandVM.Command
        {
            get
            {
                return Command;
            }
        }

        protected CommandVM(TCmdInfo cmd)
        {
            m_Command = cmd;
        }
    }
}