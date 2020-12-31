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

namespace Xarial.CadPlus.Plus.Modules
{
    public delegate void PreLoadDocumentsDelegate(ref IXDocument[] docs);
    public delegate void DisposeDocumentsCacheDelegate(IXDocument[] docs);
    public delegate void ApplyDocumentsChangesDelegate(IXDocument[] docs);

    public interface IPropertiesModule : IModule
    {
        event PreLoadDocumentsDelegate PreLoadDocuments;
        event DisposeDocumentsCacheDelegate DisposeDocumentsCache;
        event ApplyDocumentsChangesDelegate ApplyDocumentsChanges;
    }
}
