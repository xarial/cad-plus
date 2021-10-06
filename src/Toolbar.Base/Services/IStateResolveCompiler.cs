//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.CustomToolbar.Structs;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public interface IStateResolveCompiler
    {
        Dictionary<CommandMacroInfo, IToggleButtonStateResolver> CreateResolvers(IEnumerable<CommandMacroInfo> macroInfos);
    }
}
