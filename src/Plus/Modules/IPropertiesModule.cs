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
    public delegate void PreLoadDocumentsDelegate(List<IXDocument> docs);
    public delegate void DisposeDocumentsCacheDelegate(IXDocument[] docs);
    public delegate void ApplyDocumentChangesDelegate(IXDocument doc);

    public interface IPropertiesModule : IModule
    {
        event PreLoadDocumentsDelegate PreLoadDocuments;
        event DisposeDocumentsCacheDelegate DisposeDocumentsCache;
        event ApplyDocumentChangesDelegate ApplyDocumentChanges;
    }
}
