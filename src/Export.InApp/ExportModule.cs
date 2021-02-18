//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Export.InApp.Properties;
using Xarial.CadPlus.Common.Attributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;

namespace Xarial.CadPlus.Export.InApp
{
    [Module(typeof(IHostExtension))]
    public class ExportModule : IModule
    {
        [Title("eXport+")]
        [Description("Commands to export files in a batch mode")]
        [IconEx(typeof(Resources), nameof(Resources.export_vector), nameof(Resources.export_icon))]
        public enum Commands_e
        {
            [IconEx(typeof(Resources), nameof(Resources.export_vector), nameof(Resources.export_icon))]
            [Title("Open Stand-Alone...")]
            [Description("Runs stand-alone eXport+")]
            [CommandItemInfo(true, true, WorkspaceTypes_e.All)]
            RunStandAlone,
        }

        private IHostExtension m_Host;

        private IMessageService m_Msg;
        private IXLogger m_Logger;

        private IServiceProvider m_SvcProvider;

        public void Init(IHost host)
        {
            if (!(host is IHostExtension))
            {
                throw new InvalidCastException("Only extension host is supported for this module");
            }

            m_Host = (IHostExtension)host;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Connect += OnConnect;
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_SvcProvider = svcProvider;
        }

        private void OnConnect()
        {
            m_Msg = m_SvcProvider.GetService<IMessageService>();
            m_Logger = m_SvcProvider.GetService<IXLogger>();

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
        }

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec)
            {
                case Commands_e.RunStandAlone:
                    try
                    {
                        var exportPath = Path.GetFullPath(Path.Combine(
                            Path.GetDirectoryName(this.GetType().Assembly.Location), @"..\..\exportplus.exe"));

                        if (File.Exists(exportPath))
                        {
                            System.Diagnostics.Process.Start(exportPath);
                        }
                        else
                        {
                            throw new FileNotFoundException("Failed to find the path to executable");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                        m_Msg.ShowError("Failed to run Batch+");
                    }
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}
