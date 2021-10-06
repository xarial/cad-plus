//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.CustomToolbar.UI.Base;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public class CommandGroupVM : CommandVM<CommandGroupInfo>
    {
        private static readonly BitmapImage m_DefaultGroupIcon = Resources.group_icon_default.ToBitmapImage();

        private readonly CommandGroupInfo m_CmdGrp;

        public CommandsCollection<CommandMacroVM> Commands { get; }

        protected override BitmapSource DefaultIcon => m_DefaultGroupIcon;

        public CommandGroupVM()
            : this(new CommandGroupInfo(), ToolbarModule.Resolve<IIconsProvider[]>(), ToolbarModule.Resolve<IFilePathResolver>())
        {
        }

        public CommandGroupVM(CommandGroupInfo cmdGrp, IIconsProvider[] providers, IFilePathResolver filePathResolver) : base(cmdGrp, providers, filePathResolver)
        {
            m_CmdGrp = cmdGrp;
            Commands = new CommandsCollection<CommandMacroVM>(
                (cmdGrp.Commands ?? new CommandMacroInfo[0])
                .Select(c => new CommandMacroVM(c, providers, filePathResolver)));

            Commands.CommandsChanged += OnCommandsCollectionChanged;
        }

        private void OnCommandsCollectionChanged(IEnumerable<CommandMacroVM> cmds)
        {
            m_CmdGrp.Commands = cmds.Select(c => c.Command).ToArray();
        }
    }
}