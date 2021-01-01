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
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Applications
{
    public interface IDocumentHandler 
    {
        string[] SupportedFileExtensions { get; }
        IXDocument GetDocument(string path);
    }

    public interface IPropertiesApplication : IApplication
    {
        IDocumentHandler[] DocumentHandlers { get; }

        void RegisterDocumentHandler(IDocumentHandler handler);
    }
}
