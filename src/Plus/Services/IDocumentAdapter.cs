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

namespace Xarial.CadPlus.Plus.Services
{
    public interface IDocumentAdapter
    {
        IXDocument GetAdapter(IXDocument doc, bool allowReadOnly);
        void DisposeDocument(IXDocument doc);
        void ApplyChanged(IXDocument doc);
    }
}
