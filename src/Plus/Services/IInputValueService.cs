//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IInputValueServiceFactory 
    {
        IInputValueService Create(IParentWindowProvider windowProvider);
    }

    public interface IInputValueService
    {
        bool PromptInput(string title, string prompt, out string value);
    }
}