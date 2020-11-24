using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands;
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.Batch.InApp
{
    [Export(typeof(IExtensionModule))]
    public class BatchModule : IExtensionModule
    {
        //[CommandGroupInfo(CommandGroups.RootGroupId + 2)]
        //[CommandGroupParent(CommandGroups.RootGroupId)]
        [Title("Batch+")]
        [Description("Commands to batch run macros")]
        //[Icon(typeof(Resources), nameof(Resources.configure_icon))]
        public enum Commands_e
        {
            //[Icon(typeof(Resources), nameof(Resources.configure_icon))]
            [Title("Run")]
            [Description("Runs batch command to active file")]
            [CommandItemInfo(true, false, WorkspaceTypes_e.Assembly)]
            Run
        }

        private IHostExtensionApplication m_Host;

        public void Init(IHostExtensionApplication host)
        {
            m_Host = host;
            m_Host.Connect += OnConnect;
        }

        private void OnConnect()
        {
            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
        }

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec) 
            {
                case Commands_e.Run:
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}
