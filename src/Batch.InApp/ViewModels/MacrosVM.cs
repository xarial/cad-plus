﻿//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.Base.ViewModels;

namespace Xarial.CadPlus.Batch.InApp.ViewModels
{
    public class MacrosVM
    {
        public event Action AddMacros;

        public Func<string, object> PathToMacroDataConverter { get; }
            = new Func<string, object>(p => new MacroDataVM(new MacroData() { FilePath = p }));

        public Func<object, string> MacroDataToPathConverter { get; }
            = new Func<object, string>((m) => ((MacroDataVM)m).FilePath);

        public FileFilter[] MacroFilesFilter { get; }

        public ObservableCollection<MacroDataVM> Macros { get; }

        public MacrosVM(FileTypeFilter[] macroFilters) 
        {
            Macros = new ObservableCollection<MacroDataVM>();
            MacroFilesFilter = macroFilters
                .Select(f => new FileFilter(f.Name, f.Extensions))
                .Union(new FileFilter[] { XCadMacroProvider.Filter, FileFilter.AllFiles }).ToArray();
        }

        internal void RequestAddMacros() => AddMacros?.Invoke();
    }
}
