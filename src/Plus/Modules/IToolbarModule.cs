//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus.Modules
{
    public interface IIconsProvider 
    {
        FileTypeFilter Filter { get; }
        bool Matches(string filePath);
        Image GetThumbnail(string filePath);
        IXImage GetIcon(string filePath);
    }

    public enum EventType_e
    {
        ButtonClick,
        ToggleButtonCheck,
        ApplicationStart,
        DocumentNew,
        DocumentOpen,
        DocumentActivated,
        DocumentSave,
        DocumentClose,
        NewSelection,
        ConfigurationChange,
        Rebuild,
        ModelingStarted
    }

    public interface ICommandMacroInfo
    {
        string MacroPath { get; }
        string Title { get; }
        string Description { get; }
        bool UnloadAfterRun { get; }
        IMacroStartFunction EntryPoint { get; }
        string Arguments { get; }
    }

    public class MacroRunningArguments
    {
        public ICommandMacroInfo MacroInfo { get; }
        public IXDocument TargetDocument { get; }
        public bool Cancel { get; set; }

        public MacroRunningArguments(ICommandMacroInfo macroInfo, IXDocument targetDoc) 
        {
            MacroInfo = macroInfo;
            TargetDocument = targetDoc;
        }
    }

    public interface IMacroStartFunction
    {
        string ModuleName { get; }
        string SubName { get; }
    }

    public delegate void MacroRunningDelegate(EventType_e eventType, MacroRunningArguments args);

    public interface IToolbarModule : IModule
    {
        event MacroRunningDelegate MacroRunning;

        void RegisterIconsProvider(IIconsProvider provider);
    }
}
