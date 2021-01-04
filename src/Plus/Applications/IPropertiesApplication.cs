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
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Applications
{
    public interface IPropertiesApplication : IApplication
    {
        void OpenFile(string filePath);
        IDocumentProvider[] DocumentProviders { get; }
        void RegisterDocumentProvider(IDocumentProvider provider);
    }
}
