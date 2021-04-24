//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Delegates;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Applications
{
    public class PropertiesApplicationCommandManager
    {
        public static class SourceTab
        {
            public const string Name = "Source";

            public const string InputGroupName = "Input";
            public const string ScopeGroupName = "Scope";
        }
    }

    public interface IPropertiesApplication : IDocumentConsumerApplication, IHasCommandManager
    {
        Task LoadPropertiesAsync(string filePath);
    }
}
