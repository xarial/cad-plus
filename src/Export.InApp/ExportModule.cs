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

namespace Xarial.CadPlus.Export.InApp
{
    [Plus.Attributes.Module(typeof(IHostExtensionApplication))]
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

        public Guid Id => Guid.Parse("961248D6-FB9B-442C-B7ED-16C113E48AEF");

        private IHostExtensionApplication m_Host;

        private IMessageService m_Msg;
        private IXLogger m_Logger;

        public void Init(IHostApplication host)
        {
            if (!(host is IHostExtensionApplication))
            {
                throw new InvalidCastException("Only extension host is supported for this module");
            }

            m_Host = (IHostExtensionApplication)host;
            m_Host.Connect += OnConnect;
        }

        private void OnConnect()
        {
            m_Msg = m_Host.Services.GetService<IMessageService>();
            m_Logger = m_Host.Services.GetService<IXLogger>();

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
