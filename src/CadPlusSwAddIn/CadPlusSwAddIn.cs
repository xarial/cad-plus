using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.CadPlus.ExtensionModule;
using Xarial.CadPlus.XToolbar;
using Xarial.XCad.SolidWorks;

namespace Xarial.CadPlus.SwAddIn
{
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

            //TODO: add about form
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
