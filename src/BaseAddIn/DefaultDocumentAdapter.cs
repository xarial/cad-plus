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
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;

namespace Xarial.CadPlus.AddIn.Base
{
    public class DefaultDocumentAdapter : IDocumentAdapter
    {
        public void ApplyChanges(IXDocument doc)
        {
        }

        public void DisposeDocument(IXDocument doc)
        {
        }

        public IXDocument GetDocumentReplacement(IXDocument doc, bool allowReadOnly)
            => doc;
    }
}
