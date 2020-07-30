using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.CadPlus.ExtensionModule;
using Xarial.CadPlus.CustomToolbar;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;

namespace Xarial.CadPlus.SwAddIn
{
    public enum CadPlusCommands_e 
    {
        About
    }

    [ComVisible(true), Guid("6C1F130E-65C3-4237-94B7-A26B3B3AD282")]
    public class CadPlusSwAddIn : SwAddInEx
    {
        private IModule[] m_Modules;

        public override void OnConnect()
        {
            //TODO: use MEF to load modules
            m_Modules = new IModule[]
            {
                new XToolbarModule()
            };

            if (m_Modules?.Any() == true)
            {
                foreach (var module in m_Modules) 
                {
                    module.Load(this);
                }
            }

            //var cmdGrp = this.CommandManager.AddCommandGroup<CadPlusCommands_e>();
            //cmdGrp.CommandClick += OnCommandClick;

            //TODO: add about form
        }

        private void OnCommandClick(CadPlusCommands_e spec)
        {
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
