//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IDocumentAdapter
    {
        IXDocument GetDocumentReplacement(IXDocument doc, bool allowReadOnly);
        void DisposeDocument(IXDocument doc);
        void ApplyChanges(IXDocument doc);
    }
}
