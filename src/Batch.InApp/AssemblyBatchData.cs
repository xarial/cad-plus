//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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

namespace Xarial.CadPlus.Batch.InApp
{
    public class NotProcessAllFilesDependencyHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var isChecked = dependencies.First()?.GetValue();

            if (isChecked is bool) 
            {
                source.Enabled = !(bool)isChecked; 
            }
        }
    }

    [IconEx(typeof(Resources), nameof(Resources.batch_plus_assm_vector), nameof(Resources.batch_plus_assm_icon))]
    [Title("Batch+")]
    public class AssemblyBatchData
    {
        public class InputGroup 
        {
            [ControlOptions(height: 100)]
            [DependentOn(typeof(NotProcessAllFilesDependencyHandler), nameof(ProcessAllFiles))]
            [Description("List of components to run macros on")]
            public List<IXComponent> Components { get; set; }

            [Title("Process All Files")]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            [ControlTag(nameof(ProcessAllFiles))]
            public bool ProcessAllFiles { get; set; } = true;
        }

        public class MacrosGroup 
        {
            [CustomControl(typeof(MacrosList))]
            [ControlOptions(height: 100)]
            [IconEx(typeof(Resources), nameof(Resources.macros_vector), nameof(Resources.macros_icon))]
            public MacrosVM Macros { get; set; }

            [Title("Add Macros...")]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public Action AddMacros { get; }

            private readonly IMacroFileFilterProvider m_FilterProvider;

            public MacrosGroup(IMacroFileFilterProvider filterProvider) 
            {
                m_FilterProvider = filterProvider;
                AddMacros = OnAddMacros;

                Macros = new MacrosVM(m_FilterProvider);
            }

            private void OnAddMacros()
            {
                Macros.RequestAddMacros();
            }
        }

        public class OptionsGroup 
        {
            [Description("Open each document in its own window (activate)")]
            [Title("Activate Documents")]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public bool ActivateDocuments { get; set; } = true;
        }

        public InputGroup Input { get; }
        public MacrosGroup Macros { get; }
        public OptionsGroup Options { get; }

        public AssemblyBatchData(IMacroFileFilterProvider filterProvider)
        {
            Input = new InputGroup();
            Macros = new MacrosGroup(filterProvider);
            Options = new OptionsGroup();
        }
    }
}
