//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Structs;
using Xarial.CadPlus.ExtensionModule;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public partial class CommandsManager
    {
        public interface IStateResolveCompiler 
        {
            Dictionary<CommandMacroInfo, IToggleButtonStateResolver> CreateResolvers(IEnumerable<CommandMacroInfo> macroInfos);
        }
    }
}
