using System.Collections.Generic;
using Xarial.CadPlus.Plus.Modules.Toolbar;

namespace Xarial.CadPlus.Plus.Samples
{
    public class MacroInfo : ICommandMacroInfo
    {
        public string IconPath => m_SrcMacroInfo.IconPath;
        public string MacroPath => m_SrcMacroInfo.MacroPath;
        public string Title => m_SrcMacroInfo.Title;
        public string Description => m_SrcMacroInfo.Description;
        public bool UnloadAfterRun => m_SrcMacroInfo.UnloadAfterRun;
        public bool DisplayResult => m_SrcMacroInfo.DisplayResult;
        public IMacroStartFunction EntryPoint => m_SrcMacroInfo.EntryPoint;

        public IReadOnlyList<string> Arguments { get; }

        private readonly ICommandMacroInfo m_SrcMacroInfo;

        public MacroInfo(ICommandMacroInfo srcMacroInfo, IReadOnlyList<string> args) 
        {
            m_SrcMacroInfo = srcMacroInfo;
            Arguments = args;
        }
    }
}
