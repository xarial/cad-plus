//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.InApp.UI;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.CadPlus.Batch.InApp.Properties;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.InApp.ViewModels;
using Xarial.CadPlus.Common.Attributes;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Data;
using Xarial.XCad.Documents.Extensions;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.CadPlus.Plus.Modules.Batch;

namespace Xarial.CadPlus.Batch.InApp
{
    public class SelectionFilterDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var filter = dependencies.First()?.GetValue();

            if (filter is ComponentsFilter_e) 
            {
                source.Visible = (ComponentsFilter_e)filter == ComponentsFilter_e.Selection; 
            }
        }
    }

    public class FileTypeFilterDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var filter = dependencies.First()?.GetValue();

            if (filter is ComponentsFilter_e)
            {
                source.Visible = (ComponentsFilter_e)filter != ComponentsFilter_e.Selection;
            }
        }
    }

    public enum ComponentsFilter_e
    {
        [Title("Selected Components")]
        Selection,
        
        [Title("Top Level Components")]
        TopLevel,

        [Title("All Components")]
        All
    }

    [IconEx(typeof(Resources), nameof(Resources.batch_plus_assm_vector), nameof(Resources.batch_plus_assm_icon))]
    [Title("Batch+")]
    [Help("https://cadplus.xarial.com/batch/assembly/")]
    public class AssemblyBatchData
    {
        public class InputGroup 
        {
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            [ControlTag(nameof(Filter))]
            [IconEx(typeof(Resources), nameof(Resources.component_filter_vector), nameof(Resources.component_filter_icon))]
            public ComponentsFilter_e Filter { get; set; }

            [DependentOn(typeof(FileTypeFilterDependencyHandler), nameof(Filter))]
            [Description("Include part components")]
            [ControlOptions(left: 20, top: 20)]
            [BitmapButton(typeof(Resources), nameof(Resources.part_icon), 30, 30)]
            public bool FilterParts { get; set; }

            [DependentOn(typeof(FileTypeFilterDependencyHandler), nameof(Filter))]
            [Description("Include assembly components")]
            [ControlOptions(left: 45, top: 20)]
            [BitmapButton(typeof(Resources), nameof(Resources.assembly_icon), 30, 30)]
            public bool FilterAssemblies { get; set; }

            [ControlOptions(height: 100)]
            [DependentOn(typeof(SelectionFilterDependencyHandler), nameof(Filter))]
            [Description("List of components to run macros on")]
            public List<IXComponent> Components { get; set; }

            public InputGroup() 
            {
                Filter = ComponentsFilter_e.All;
                FilterParts = true;
                FilterAssemblies = true;
            }
        }

        public class MacrosGroup 
        {
            [CustomControl(typeof(MacrosList))]
            [ControlOptions(height: 80)]
            [IconEx(typeof(Resources), nameof(Resources.macros_vector), nameof(Resources.macros_icon))]
            public MacrosVM Macros { get; }

            [Title("Add Macros...")]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public Action AddMacros { get; }

            public MacrosGroup(FileTypeFilter[] macroFilters) 
            {
                AddMacros = OnAddMacros;

                Macros = new MacrosVM(macroFilters);
            }

            private void OnAddMacros()
                => Macros.RequestAddMacros();
        }

        public class OptionsGroup 
        {
            [Description("Open each document in its own window (activate)")]
            [ControlOptions(left: 20, top: 0)]
            [BitmapButton(typeof(Resources), nameof(Resources.activate_document), 30, 30)]
            public bool ActivateDocuments { get; set; } = true;

            [Description("Allow opening documents which are not currently loaded into memory as read-only")]
            [ControlOptions(left: 45, top: 0)]
            [BitmapButton(typeof(Resources), nameof(Resources.read_only_mode), 30, 30)]
            public bool AllowReadOnly { get; set; } = false;

            [Description("Allow opening documents which are not currently loaded into memory in a rapid mode")]
            [ControlOptions(left: 70, top: 0)]
            [BitmapButton(typeof(Resources), nameof(Resources.rapid_mode), 30, 30)]
            public bool AllowRapid { get; set; } = false;

            [Description("Save documents automatically after running the macros")]
            [ControlOptions(left: 20, top: 25)]
            [BitmapButton(typeof(Resources), nameof(Resources.auto_save_docs), 30, 30)]
            public bool AutoSave { get; set; } = false;

            [Description("Automatically close all popup windows")]
            [ControlOptions(left: 45, top: 25)]
            [BitmapButton(typeof(Resources), nameof(Resources.silent_mode), 30, 30)]
            public bool Silent { get; set; }

            [DynamicControls(BatchModuleGroup_e.Options)]
            public List<IRibbonCommand> AdditionalCommands { get; }

            public OptionsGroup() 
            {
                AdditionalCommands = new List<IRibbonCommand>();
            }
        }

        public InputGroup Input { get; }
        public MacrosGroup Macros { get; }
        public OptionsGroup Options { get; }

        public AssemblyBatchData(ICadDescriptor cadEntDesc)
        {
            Input = new InputGroup();
            Macros = new MacrosGroup(cadEntDesc.MacroFileFilters);
            Options = new OptionsGroup();
        }
    }
}
