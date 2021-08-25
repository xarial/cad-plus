//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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

    public enum TriggerType_e
    {
        Button,
        ToggleButton,
        ApplicationStart,
        DocumentNew,
        DocumentOpen,
        DocumentActivated,
        DocumentSave,
        DocumentClose,
        NewSelection,
        ConfigurationChange,
        Rebuild,
    }

    public class TriggerInvokingArguments
    {
        public string MacroPath { get; set; }
        public IMacroStartFunction EntryPoint { get; set; }
        public bool UnloadAfterRun { get; set; }
        public string Arguments { get; set; }
        public bool Cancel { get; set; }
    }

    public interface IMacroStartFunction
    {
        string ModuleName { get; set; }
        string SubName { get; set; }
    }

    public delegate void TriggerInvokingDelegate(TriggerType_e triggerType, TriggerInvokingArguments args);

    public interface IToolbarModule : IModule
    {
        event TriggerInvokingDelegate TriggerInvoking;

        void RegisterIconsProvider(IIconsProvider provider);
    }
}
