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

namespace Xarial.CadPlus.SwAddIn
{
    [CommandGroupInfo(CommandGroups.RootGroupId)]
    [Title("CAD+ Toolset")]
    [Description("CAD+ Toolset features and options")]
    public enum CadPlusCommands_e 
    {
        [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
        About
    }

    [ComVisible(true), Guid("6C1F130E-65C3-4237-94B7-A26B3B3AD282")]
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
                case CadPlusCommands_e.About:
                    //TODO: add about form
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
