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
using Xarial.XToolkit.Reporting;
using Xarial.XCad.Base;
using Xarial.CadPlus.Batch.InApp.UI;
using Xarial.CadPlus.Batch.InApp.Properties;
using System.IO;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Documents.Enums;
using Xarial.CadPlus.Common;
using Xarial.CadPlus.Common.Attributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class ComponentDocumentSafeEqualityComparer : IEqualityComparer<IXComponent>
    {
        public bool Equals(IXComponent x, IXComponent y)
        {
            try
            {
                return string.Equals(x.Path, y.Path, StringComparison.CurrentCultureIgnoreCase);
            }
            catch 
            {
                return false;
            }
        }

        public int GetHashCode(IXComponent obj) => 0;
    }

    [Module(typeof(IHostExtension))]
    public class BatchModule : IBatchInAppModule
    {
        public event ProcessBatchInputDelegate ProcessInput;

        [Title("Batch+")]
        [Description("Commands to batch run macros")]
        [IconEx(typeof(Resources), nameof(Resources.batch_plus_vector), nameof(Resources.batch_plus_icon))]
        public enum Commands_e
        {
            [IconEx(typeof(Resources), nameof(Resources.batch_plus_vector), nameof(Resources.batch_plus_icon))]
            [Title("Open Stand-Alone...")]
            [Description("Runs stand-alone Batch+")]
            [CommandItemInfo(true, true, WorkspaceTypes_e.All)]
            RunStandAlone,

            [IconEx(typeof(Resources), nameof(Resources.batch_plus_assm_vector), nameof(Resources.batch_plus_assm_icon))]
            [Title("Run")]
            [Description("Runs batch command to active file")]
            [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly)]
            RunInApp
        }

        public Guid Id => Guid.Parse(ModuleIds.BatchInApp);

        private IHostExtension m_Host;

        private IXPropertyPage<AssemblyBatchData> m_Page;
        private AssemblyBatchData m_Data;

        private IMacroRunnerExService m_MacroRunnerSvc;
        private IMessageService m_Msg;
        private IXLogger m_Logger;

        public void Init(IHost host)
        {
            if (!(host is IHostExtension))
            {
                throw new InvalidCastException("Only extension host is supported for this module");
            }

            m_Host = (IHostExtension)host;
            m_Host.Connect += OnConnect;
        }

        private void OnConnect()
        {
            m_MacroRunnerSvc = m_Host.Services.GetService<IMacroRunnerExService>();
            m_Msg = m_Host.Services.GetService<IMessageService>();
            m_Logger = m_Host.Services.GetService<IXLogger>();

            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            m_Page = m_Host.CreatePage<AssemblyBatchData>();
            m_Data = new AssemblyBatchData(m_Host.Services.GetService<IMacroFileFilterProvider>());
            m_Page.Closing += OnPageClosing;
            m_Page.Closed += OnPageClosed;
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                if (!m_Data.Macros.Macros.Macros.Any()) 
                {
                    arg.Cancel = true;
                    arg.ErrorMessage = "Select macros to run";
                }
                
                if (!m_Data.Input.Components.Any() && !m_Data.Input.ProcessAllFiles) 
                {
                    arg.Cancel = true;
                    arg.ErrorMessage = "Select components to process";
                }
            }
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    IXDocument[] docs = null;
                    var assm = m_Host.Extension.Application.Documents.Active as IXAssembly;

                    if (m_Data.Input.ProcessAllFiles)
                    {
                        docs = assm.Dependencies;
                    }
                    else
                    {
                        docs = m_Data.Input.Components
                            .Distinct(new ComponentDocumentSafeEqualityComparer())
                            .Select(c => c.Document).ToArray();
                    }

                    var exec = new AssemblyBatchRunJobExecutor(m_Host.Extension.Application, m_MacroRunnerSvc,
                        docs, m_Data.Macros.Macros.Macros, m_Data.Options.ActivateDocuments);
                    
                    var vm = new JobResultVM(assm.Title, exec);

                    exec.ExecuteAsync().Wait();
                    
                    var wnd = m_Host.Extension.CreatePopupWindow<ResultsWindow>();
                    wnd.Control.Title = $"{assm.Title} batch job result";
                    wnd.Control.DataContext = vm;
                    wnd.Show();
                }
                catch (Exception ex)
                {
                    m_Msg.ShowError(ex.ParseUserError(out string callStack));
                    m_Logger.Log(callStack);
                }
            }
        }

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec) 
            {
                case Commands_e.RunStandAlone:
                    try
                    {
                        var batchPath = Path.GetFullPath(Path.Combine(
                            Path.GetDirectoryName(this.GetType().Assembly.Location), @"..\..\batchplus.exe"));

                        if (File.Exists(batchPath))
                        {
                            System.Diagnostics.Process.Start(batchPath);
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

                case Commands_e.RunInApp:
                    m_Data.Input.Components = m_Host.Extension.Application.Documents.Active.Selections.OfType<IXComponent>().ToList();
                    m_Page.Show(m_Data);
                    break;
            }
        }

        public void AddCommands(params IRibbonCommand[] cmd)
        {
        }

        public void Dispose()
        {
        }
    }
}
