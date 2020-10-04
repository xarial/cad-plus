//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.AddIn.Base.Properties;
using Xarial.CadPlus.CustomToolbar;
using Xarial.CadPlus.ExtensionModule;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.AddIn.Base
{
    public class AddInController : IDisposable
    {
        private readonly IXExtension m_Ext;
        private readonly IModule[] m_Modules;

        public AddInController(IXExtension ext) 
        {
            m_Ext = ext;

            var cmdGrp = ext.CommandManager.AddCommandGroup<CadPlusCommands_e>();
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
                    module.Load(ext);
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
                        m_Ext.Application.WindowHandle);
                    break;
            }
        }
        
        public void Dispose()
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
