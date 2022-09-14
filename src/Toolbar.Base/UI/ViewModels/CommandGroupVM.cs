//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Xarial.CadPlus.CustomToolbar.Services;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.CustomToolbar.UI.Base;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Toolbar.Properties;
using Xarial.CadPlus.Toolbar.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.CustomToolbar.UI.ViewModels
{
    public interface ICommandGroupVMFactory
    {
        CommandGroupVM Create(CommandGroupInfo cmdGrp);
    }

    public class CommandGroupVMFactory : ICommandGroupVMFactory
    {
        private readonly IIconsProvider[] m_IconsProviders;
        private readonly IFilePathResolver m_FilePathResolver;

        private readonly ICommandMacroVMFactory m_MacroVmFactory;

        public CommandGroupVMFactory(IIconsProvider[] iconsProviders, IFilePathResolver filePathResolver, ICommandMacroVMFactory macroVmFactory) 
        {
            m_MacroVmFactory = macroVmFactory;

            m_IconsProviders = iconsProviders;
            m_FilePathResolver = filePathResolver;
        }

        public CommandGroupVM Create(CommandGroupInfo cmdGrp) => new CommandGroupVM(cmdGrp, m_IconsProviders, m_FilePathResolver, m_MacroVmFactory);
    }

    public class CommandGroupVM : CommandVM<CommandGroupInfo>
    {
        private static readonly BitmapImage m_DefaultGroupIcon = Resources.group_icon_default.ToBitmapImage();

        private readonly CommandGroupInfo m_CmdGrp;

        public CommandsCollection<CommandMacroVM> Commands { get; }

        protected override BitmapSource DefaultIcon => m_DefaultGroupIcon;

        private readonly ICommandMacroVMFactory m_MacroVmFactory;

        public CommandGroupVM(CommandGroupInfo cmdGrp, IIconsProvider[] providers, IFilePathResolver filePathResolver, ICommandMacroVMFactory macroVmFactory) 
            : base(cmdGrp, providers, filePathResolver)
        {
            m_MacroVmFactory = macroVmFactory;
            m_CmdGrp = cmdGrp;

            Commands = new CommandsCollection<CommandMacroVM>((cmdGrp.Commands ?? new CommandMacroInfo[0])
                .Select(c => m_MacroVmFactory.Create(c)),
                () => m_MacroVmFactory.Create(new CommandMacroInfo()));

            Commands.CommandsChanged += OnCommandsCollectionChanged;
        }

        private void OnCommandsCollectionChanged(IEnumerable<CommandMacroVM> cmds)
        {
            m_CmdGrp.Commands = cmds.Select(c => c.Command).ToArray();
        }
    }
}