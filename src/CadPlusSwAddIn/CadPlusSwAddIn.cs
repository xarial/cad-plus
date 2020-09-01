//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.CadPlus.ExtensionModule;
using Xarial.CadPlus.CustomToolbar;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.Base.Attributes;
using System.ComponentModel;
using Xarial.CadPlus.SwAddIn.Properties;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.SwAddIn
{
    [CommandGroupInfo(CommandGroups.RootGroupId)]
    [Title("CAD+ Toolset")]
    [Description("CAD+ Toolset features and options")]
    public enum CadPlusCommands_e 
    {
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [Icon(typeof(Resources), nameof(Resources.help_icon))]
        Help,

        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        [Icon(typeof(Resources), nameof(Resources.about_icon))]
        About
    }

    [ComVisible(true), Guid("AC45BDF0-66CB-4B08-8127-06C1F0C9452F")]
    [Title("CAD+ Toolset")]
    [Description("The toolset of utilities to complement SOLIDWORKS functionality")]
    public class CadPlusSwAddIn : SwAddInEx
    {
        private IModule[] m_Modules;

        public override void OnConnect()
        {
            var cmdGrp = this.CommandManager.AddCommandGroup<CadPlusCommands_e>();
            cmdGrp.CommandClick += OnCommandClick;

            //TODO: use MEF to load modules
            m_Modules = new IModule[]
            {
                new CustomToolbarModule()
            };

            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules) 
                {
                    module.Load(this);
                }
            }
        }

        private void OnCommandClick(CadPlusCommands_e spec)
        {
            switch (spec) 
            {
                case CadPlusCommands_e.Help:
                    try
                    {
                        System.Diagnostics.Process.Start(Resources.HelpLink);
                    }
                    catch 
                    { 
                    }
                    break;

                case CadPlusCommands_e.About:
                    AboutDialog.Show(this.GetType().Assembly, Resources.logo,
                        (IntPtr)Application.Sw.IFrameObject().GetHWndx64());
                    break;
            }
        }

        public override void OnDisconnect()
        {
            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules)
                {
                    module.Dispose();
                }
            }
        }
    }
}
