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
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Documents;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.Batch.InApp
{
    [Export(typeof(IExtensionModule))]
    public class BatchModule : IExtensionModule
    {
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

        private IXPropertyPage<AssemblyBatchData> m_Page;
        private AssemblyBatchData m_Data;

        private IMacroRunnerExService m_MacroRunnerSvc;

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
            m_MacroRunnerSvc = (IMacroRunnerExService)m_Host.Services.GetService(typeof(IMacroRunnerExService));

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            m_Page = m_Host.CreatePage<AssemblyBatchData>();
            m_Data = new AssemblyBatchData();
            m_Page.Closed += OnPageClosed;
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                IEnumerable<IXComponent> comps = null;

                if (m_Data.ProcessAllFiles)
                {
                    comps = (m_Host.Extension.Application.Documents.Active as IXAssembly).Components.Flatten();
                }
                else 
                {
                    comps = m_Data.Components;
                }

                comps = comps.Distinct(new ComponentPathEqualityComparer());
                
                var exec = new AssemblyBatchRunJobExecutor(m_Host.Extension.Application, m_MacroRunnerSvc,
                    comps.ToArray(), m_Data.Macros, m_Data.ActivateDocuments);

                exec.ExecuteAsync();
            }
        }

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec) 
            {
                case Commands_e.Run:
                    m_Data.Components = m_Host.Extension.Application.Documents.Active.Selections.OfType<IXComponent>().ToList();
                    m_Page.Show(m_Data);
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}
