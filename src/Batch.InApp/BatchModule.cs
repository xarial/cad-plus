//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.CadPlus.Plus;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using Xarial.CadPlus.Batch.InApp.UI;
using Xarial.CadPlus.Batch.InApp.Properties;
using System.IO;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.CadPlus.Batch.InApp.Controls;
using System.Windows.Threading;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using System.Threading;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.CadPlus.Plus.Shared.Helpers;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Batch.InApp
{
    internal class ComponentDocumentSafeEqualityComparer : IEqualityComparer<IXComponent>
    {
        public bool Equals(IXComponent x, IXComponent y)
        {
            try
            {
                return string.Equals(x.ReferencedDocument.Path, y.ReferencedDocument.Path, StringComparison.CurrentCultureIgnoreCase);
            }
            catch 
            {
                return false;
            }
        }

        public int GetHashCode(IXComponent obj) => 0;
    }

    [Title("Batch+")]
    [Description("Commands to batch run macros")]
    [IconEx(typeof(Resources), nameof(Resources.batch_plus_vector), nameof(Resources.batch_plus_icon))]
    [CommandGroupInfo((int)CadCommandGroupIds_e.Batch)]
    [CommandOrder(4)]
    public enum Commands_e
    {
        [IconEx(typeof(Resources), nameof(Resources.batch_plus_vector), nameof(Resources.batch_plus_icon))]
        [Title("Open Batch+ Stand-Alone...")]
        [Description("Runs stand-alone Batch+")]
        [CommandItemInfo(true, true, WorkspaceTypes_e.All)]
        RunStandAlone,

        [IconEx(typeof(Resources), nameof(Resources.batch_plus_assm_vector), nameof(Resources.batch_plus_assm_icon))]
        [Title("Batch Run Macros")]
        [Description("Runs batch command to active file")]
        [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly, true)]
        RunInApp
    }

    [Module(typeof(IHostCadExtension))]
    public class BatchModule : IBatchInAppModule
    {
        public event ProcessInAppBatchInputDelegate ProcessInput;

        private IHostCadExtension m_Host;

        private IXPropertyPage<AssemblyBatchData> m_Page;
        private AssemblyBatchData m_Data;

        private IMacroExecutor m_MacroRunnerSvc;
        private IMessageService m_MsgSvc;
        private IXLogger m_Logger;
        private IMacroRunnerPopupHandlerFactory m_PopupHandlerFact;

        private ICadDescriptor m_CadDesc;

        private BatchJobHandlersRepository m_BatchJobHandlerRepo;

        public void Init(IHost host)
        {
            if (!(host is IHostCadExtension))
            {
                throw new InvalidCastException("Only extension host is supported for this module");
            }

            m_Host = (IHostCadExtension)host;
            m_Host.ConfigureServices += OnConfigureServices;
            m_Host.Connect += OnConnect;
            m_Host.Initialized += OnHostInitialized;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.RegisterSingleton<IMacroRunnerPopupHandlerFactory, MacroRunnerPopupHandlerFactory>();
        }

        private void OnHostInitialized(IApplication app, IServiceProvider svcProvider, IModule[] modules)
        {
            m_MacroRunnerSvc = svcProvider.GetService<IMacroExecutor>();
            m_MsgSvc = svcProvider.GetService<IMessageService>();
            m_Logger = svcProvider.GetService<IXLogger>();
            m_PopupHandlerFact = svcProvider.GetService<IMacroRunnerPopupHandlerFactory>();
            m_CadDesc = svcProvider.GetService<ICadDescriptor>();

            m_Data = new AssemblyBatchData(m_CadDesc);

            m_BatchJobHandlerRepo = new BatchJobHandlersRepository(svcProvider.GetService<IBatchJobHandlerServiceFactory>(), m_Logger);
        }

        private void OnConnect()
        {
            m_Host.RegisterCommands<Commands_e>(OnCommandClick);
            m_Page = m_Host.Extension.CreatePage<AssemblyBatchData>(CreateDynamicPageControls);
            m_Page.Closing += OnPageClosing;
            m_Page.Closed += OnPageClosed;
        }

        private IControlDescriptor[] CreateDynamicPageControls(object tag)
        {
            var grp = (BatchModuleGroup_e)tag;

            switch (grp) 
            {
                case BatchModuleGroup_e.Options:
                    return CreateControls(m_Data.Options.AdditionalCommands);

                default:
                    throw new NotSupportedException();

            }
        }

        private IControlDescriptor[] CreateControls(IEnumerable<IRibbonCommand> cmds) 
        {
            var ctrls = new List<IControlDescriptor>();

            foreach (var cmd in cmds) 
            {
                switch (cmd) 
                {
                    case IRibbonToggleCommand toggle:
                        ctrls.Add(new BaseControlDescriptor<List<IRibbonCommand>, bool, IRibbonToggleCommand>(
                            toggle,
                            ctx => toggle.Value,
                            (ctx, v) => toggle.Value = v));
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return ctrls.ToArray();
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    ValidateData();
                }
                catch (Exception ex)
                {
                    arg.Cancel = true;
                    arg.ErrorMessage = ex.ParseUserError();
                }
            }
        }

        private void ValidateData()
        {
            if (!m_Data.Macros.Macros.Macros.Any())
            {
                throw new UserException("Select macros to run");
            }

            if (m_Data.Input.Filter == ComponentsFilter_e.Selection)
            {
                if (!m_Data.Input.Components?.Any() == true)
                {
                    throw new UserException("Select components to process");
                }
            }
            else 
            {
                if (!m_Data.Input.FilterParts && !m_Data.Input.FilterAssemblies) 
                {
                    throw new UserException("Select filter part and/or assembly file type filter");
                }
            }
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                try
                {
                    var assm = (IXAssembly)m_Host.Extension.Application.Documents.Active;

                    IEnumerable<IXComponent> inputComps;

                    if (m_Data.Input.Filter == ComponentsFilter_e.Selection)
                    {
                        inputComps = m_Data.Input.Components;
                    }
                    else 
                    {
                        switch (m_Data.Input.Filter)
                        {
                            case ComponentsFilter_e.All:
                                inputComps = assm.Configurations.Active.Components.Flatten();
                                break;

                            case ComponentsFilter_e.TopLevel:
                                inputComps = assm.Configurations.Active.Components;
                                break;

                            default:
                                throw new NotSupportedException();
                        }

                        inputComps = inputComps.Where(c => (m_Data.Input.FilterParts && c is IXPartComponent) || (m_Data.Input.FilterAssemblies && c is IXAssemblyComponent));
                    }

                    var input = inputComps.Distinct(new ComponentDocumentSafeEqualityComparer())
                        .Select<IXComponent, IXDocument>(c => c.ReferencedDocument).ToList();

                    ProcessInput?.Invoke(m_Host.Extension.Application, input);

                    var doc = m_Host.Extension.Application.Documents.Active;

                    var job = new BatchMacroRunJobAssembly(m_Host.Extension.Application, m_MacroRunnerSvc, m_CadDesc,
                        input.ToArray(), m_Logger, m_Data.Macros.Macros.Macros.Select(x => x.Data).ToArray(),
                        m_Data.Options.ActivateDocuments, m_Data.Options.AllowReadOnly,
                        m_Data.Options.AllowRapid, m_Data.Options.AutoSave, m_PopupHandlerFact.Create(m_Data.Options.Silent));

                    m_BatchJobHandlerRepo.RunNew(job, $"Batch Macro Running: {doc.Title}");
                }
                catch (Exception ex)
                {
                    m_MsgSvc.ShowError(ex);
                    m_Logger.Log(ex);
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
                        m_MsgSvc.ShowError("Failed to run Batch+");
                    }
                    break;

                case Commands_e.RunInApp:
                    m_Page.Show(m_Data);
                    break;
            }
        }

        public void AddCommands(BatchModuleGroup_e group, params IRibbonCommand[] cmd)
        {
            switch (group) 
            {
                case BatchModuleGroup_e.Options:
                    m_Data.Options.AdditionalCommands.AddRange(cmd);
                    break;
            }
        }

        public void Dispose()
        {
            m_BatchJobHandlerRepo.Dispose();
        }
    }
}
